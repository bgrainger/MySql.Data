﻿using System;
using System.Data;
using System.Data.Common;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.Serialization;

namespace MySql.Data.MySqlClient
{
	public sealed class MySqlConnection : DbConnection
	{
		public MySqlConnection()
		{
			m_connectionStringBuilder = new MySqlConnectionStringBuilder();
		}

		public MySqlConnection(string connectionString)
			: this()
		{
			ConnectionString = connectionString;
		}

		public new MySqlTransaction BeginTransaction()
		{
			return (MySqlTransaction) base.BeginTransaction();
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			if (State != ConnectionState.Open)
				throw new InvalidOperationException("Connection is not open.");
			if (CurrentTransaction != null)
				throw new InvalidOperationException("Transactions may not be nested.");

			string isolationLevelValue;
			switch (isolationLevel)
			{
			case IsolationLevel.ReadUncommitted:
				isolationLevelValue = "read uncommitted";
				break;

			case IsolationLevel.ReadCommitted:
				isolationLevelValue = "read committed";
				break;

			case IsolationLevel.Unspecified:
			// "In terms of the SQL:1992 transaction isolation levels, the default InnoDB level is REPEATABLE READ." - http://dev.mysql.com/doc/refman/5.7/en/innodb-transaction-model.html
			case IsolationLevel.RepeatableRead:
				isolationLevelValue = "repeatable read";
				break;

			case IsolationLevel.Serializable:
				isolationLevelValue = "serializable";
				break;

			case IsolationLevel.Chaos:
			case IsolationLevel.Snapshot:
			default:
				throw new NotSupportedException("IsolationLevel.{0} is not supported.".FormatInvariant(isolationLevel));
			}

			using (var cmd = new MySqlCommand("set session transaction isolation level " + isolationLevelValue + "; start transaction;", this))
				cmd.ExecuteNonQuery();

			var transaction = new MySqlTransaction(this, isolationLevel);
			CurrentTransaction = transaction;
			return transaction;
		}

#if !NETSTANDARD1_3
		public override void EnlistTransaction(System.Transactions.Transaction transaction)
		{
			throw new NotSupportedException("System.Transactions.Transaction is not supported. Use BeginTransaction instead.");
		}
#endif

		public override void Close()
		{
			DoClose();
		}

		public override void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public override void Open()
		{
			OpenAsync(CancellationToken.None).GetAwaiter().GetResult();
		}

		public override async Task OpenAsync(CancellationToken cancellationToken)
		{
			VerifyNotDisposed();
			if (State != ConnectionState.Closed)
				throw new InvalidOperationException("Cannot Open when State is {0}.".FormatInvariant(State));
#if !NETSTANDARD1_3
			if (System.Transactions.Transaction.Current != null)
				throw new NotSupportedException("Ambient transactions are not supported. Use BeginTransaction instead.");
#endif

			if (m_connectionStringBuilder.UseCompression)
				throw new NotSupportedException("Compression not supported.");

			SetState(ConnectionState.Connecting);

			bool success = false;
			try
			{
				var pool = ConnectionPool.GetPool(m_connectionStringBuilder);
				m_session = pool?.TryGetSession();
				if (m_session == null)
				{
					m_session = new MySqlSession(pool);
					var connected = await AdaptTask(m_session.ConnectAsync(m_connectionStringBuilder.Server.Split(','), (int) m_connectionStringBuilder.Port)).ConfigureAwait(false);
					if (!connected)
					{
						SetState(ConnectionState.Closed);
						throw new MySqlException("Unable to connect to any of the specified MySQL hosts.");
					}

					var payload = await AdaptTask(m_session.ReceiveAsync(cancellationToken)).ConfigureAwait(false);
					var reader = new ByteArrayReader(payload.ArraySegment.Array, payload.ArraySegment.Offset, payload.ArraySegment.Count);
					var initialHandshake = new InitialHandshakePacket(reader);
					if (initialHandshake.AuthPluginName != "mysql_native_password")
						throw new NotSupportedException("Only 'mysql_native_password' authentication method is supported.");
					m_session.ServerVersion = new ServerVersion(Encoding.ASCII.GetString(initialHandshake.ServerVersion));

					var response = HandshakeResponse41Packet.Create(initialHandshake, m_connectionStringBuilder.UserID, m_connectionStringBuilder.Password, m_database);
					payload = new PayloadData(new ArraySegment<byte>(response));
					await AdaptTask(m_session.SendReplyAsync(payload, cancellationToken)).ConfigureAwait(false);
					await AdaptTask(m_session.ReceiveReplyAsync(cancellationToken)).ConfigureAwait(false);
					// TODO: Check success
				}
				else if (m_connectionStringBuilder.ConnectionReset)
				{
					if (m_session.ServerVersion.Version.CompareTo(ServerVersions.SupportsResetConnection) >= 0)
					{
						await AdaptTask(m_session.SendAsync(ResetConnectionPayload.Create(), cancellationToken)).ConfigureAwait(false);
						var payload = await AdaptTask(m_session.ReceiveReplyAsync(cancellationToken));
						OkPayload.Create(payload);
					}
					else
					{
						// MySQL doesn't appear to accept a replayed hashed password (using the challenge from the initial handshake), so just send zeroes
						// and expect to get a new challenge
						var payload = ChangeUserPayload.Create(m_connectionStringBuilder.UserID, new byte[20], m_database);
						await AdaptTask(m_session.SendAsync(payload, cancellationToken)).ConfigureAwait(false);
						payload = await AdaptTask(m_session.ReceiveReplyAsync(cancellationToken)).ConfigureAwait(false);
						var switchRequest = AuthenticationMethodSwitchRequestPayload.Create(payload);
						if (switchRequest.Name != "mysql_native_password")
							throw new NotSupportedException("Only 'mysql_native_password' authentication method is supported.");
						var hashedPassword = AuthenticationUtility.HashPassword(switchRequest.Data, 0, m_connectionStringBuilder.Password);
						payload = new PayloadData(new ArraySegment<byte>(hashedPassword));
						await AdaptTask(m_session.SendReplyAsync(payload, cancellationToken)).ConfigureAwait(false);
						payload = await AdaptTask(m_session.ReceiveReplyAsync(cancellationToken)).ConfigureAwait(false);
						OkPayload.Create(payload);
					}
				}

				m_hasBeenOpened = true;
				SetState(ConnectionState.Open);
				success = true;
			}
			catch (MySqlException)
			{
				SetState(ConnectionState.Closed);
				throw;
			}
			catch (SocketException ex)
			{
				SetState(ConnectionState.Closed);
				throw new MySqlException("Unable to connect to any of the specified MySQL hosts.", ex);
			}
			finally
			{
				if (!success)
					Utility.Dispose(ref m_session);
			}
		}

		public override string ConnectionString
		{
			get
			{
				return m_connectionStringBuilder.GetConnectionString(!m_hasBeenOpened || m_connectionStringBuilder.PersistSecurityInfo);
			}
			set
			{
				m_connectionStringBuilder = new MySqlConnectionStringBuilder(value);
				m_database = m_connectionStringBuilder.Database;
			}
		}

		public override string Database => m_database;

		public override ConnectionState State => m_connectionState;

		public override string DataSource => m_connectionStringBuilder.Server;

		public override string ServerVersion => m_session.ServerVersion.OriginalString;

		protected override DbCommand CreateDbCommand()
		{
			return new MySqlCommand(this, CurrentTransaction);
		}

		public override int ConnectionTimeout
		{
			get { throw new NotImplementedException(); }
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					DoClose();
			}
			finally
			{
				m_isDisposed = true;
				base.Dispose(disposing);
			}
		}

		internal MySqlSession Session
		{
			get
			{
				VerifyNotDisposed();
				return m_session;
			}
		}

		internal MySqlTransaction CurrentTransaction { get; set; }
		internal bool HasActiveReader { get; set; }
		internal bool AllowUserVariables => m_connectionStringBuilder.AllowUserVariables;
		internal bool ConvertZeroDateTime => m_connectionStringBuilder.ConvertZeroDateTime;
		internal bool OldGuids => m_connectionStringBuilder.OldGuids;
		internal bool Synchronous => m_connectionStringBuilder.Synchronous;

		internal Task AdaptTask(Task task)
		{
			if (!Synchronous)
				return task;

			task.GetAwaiter().GetResult();
			return Task.FromResult<object>(null);
		}

		internal Task<T> AdaptTask<T>(Task<T> task)
		{
			if (!Synchronous)
				return task;

			return Task.FromResult(task.GetAwaiter().GetResult());
		}

		internal ValueTask<T> AdaptTask<T>(ValueTask<T> task)
		{
			if (!Synchronous || task.IsCompletedSuccessfully)
				return task;

			return new ValueTask<T>(task.AsTask().GetAwaiter().GetResult());
		}

		private void SetState(ConnectionState newState)
		{
			if (m_connectionState != newState)
			{
				var previousState = m_connectionState;
				m_connectionState = newState;
				OnStateChange(new StateChangeEventArgs(previousState, newState));
			}
		}

		private void VerifyNotDisposed()
		{
			if (m_isDisposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		private void DoClose()
		{
			if (m_connectionState != ConnectionState.Closed)
			{
				if (CurrentTransaction != null)
				{
					CurrentTransaction.Dispose();
					CurrentTransaction = null;
				}
				if (m_session != null)
				{
					if (!m_session.ReturnToPool())
						m_session.Dispose();
					m_session = null;
				}
				SetState(ConnectionState.Closed);
			}
		}

		MySqlConnectionStringBuilder m_connectionStringBuilder;
		MySqlSession m_session;
		ConnectionState m_connectionState;
		bool m_hasBeenOpened;
		bool m_isDisposed;
		string m_database;
	}
}
