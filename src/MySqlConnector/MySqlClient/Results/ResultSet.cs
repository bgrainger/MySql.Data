﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.Protocol.Serialization;
using MySql.Data.Serialization;

namespace MySql.Data.MySqlClient.Results
{
	internal class ResultSet
	{
		public ResultSet(MySqlDataReader dataReader)
		{
			DataReader = dataReader;
		}

		public async Task<ResultSet> ReadResultSetHeaderAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			// ResultSet can be re-used, so initialize everything
			BufferState = ResultSetState.None;
			ColumnDefinitions = null;
			LastInsertId = 0;
			RecordsAffected = 0;
			State = ResultSetState.None;
			m_dataLengths = null;
			m_dataOffsets = null;
			m_readBuffer.Clear();
			m_row = null;
			m_rowBuffered = null;

			while (true)
			{
				var payload = await Session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(false);

				var firstByte = payload.HeaderByte;
				if (firstByte == OkPayload.Signature)
				{
					var ok = OkPayload.Create(payload);
					RecordsAffected = ok.AffectedRowCount;
					LastInsertId = ok.LastInsertId;
					ColumnDefinitions = null;
					State = (ok.ServerStatus & ServerStatus.MoreResultsExist) == 0
						? ResultSetState.NoMoreData
						: ResultSetState.HasMoreData;
					if (State == ResultSetState.NoMoreData)
						break;
				}
                else if (firstByte == LocalInfilePayload.Signature)
                {
                    var localInfile = LocalInfilePayload.Create(payload);
                    Stream infileStream;
                    try
                    {
                        if (localInfile.FileName.StartsWith(LocalInfilePayload.InfileStreamPrefix))
                        {
                            infileStream = MySqlBulkLoader.GetInfileStreamByKey(localInfile.FileName);
                        }
                        else
                        {
                            infileStream = new FileStream(localInfile.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                        }
                        using (infileStream)
                        {
                            byte[] readBuffer = new byte[8192];
                            int byteCount;
                            while ((byteCount = await infileStream.ReadAsync(readBuffer, 0, 8192)) > 0)
                            {
                                payload = new PayloadData(new ArraySegment<byte>(readBuffer, 0, byteCount));
                                await Session.SendReplyAsync(payload, ioBehavior, cancellationToken);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        throw new MySqlException("Error during LOAD DATA LOCAL INFILE", exc);
                    }
                    finally
                    {
                        await Session.SendReplyAsync(EmptyPayload.Create(), ioBehavior, cancellationToken);
                    }
                }
                else
                {
					var reader = new ByteArrayReader(payload.ArraySegment);
					var columnCount = (int) reader.ReadLengthEncodedInteger();
					ColumnDefinitions = new ColumnDefinitionPayload[columnCount];
					m_dataOffsets = new int[columnCount];
					m_dataLengths = new int[columnCount];

					for (var column = 0; column < ColumnDefinitions.Length; column++)
					{
						payload = await Session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
						ColumnDefinitions[column] = ColumnDefinitionPayload.Create(payload);
					}

					payload = await Session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
					EofPayload.Create(payload);

					LastInsertId = -1;
					State = ResultSetState.ReadResultSetHeader;
					break;
				}
			}
			BufferState = State;
			return this;
		}

		public async Task BufferEntireAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			while (BufferState == ResultSetState.ReadingRows || BufferState == ResultSetState.ReadResultSetHeader)
				await BufferReadAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
		}

		public async Task ReadEntireAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			while (State == ResultSetState.ReadingRows || State == ResultSetState.ReadResultSetHeader)
				await ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
		}

		public bool Read()
		{
			return ReadAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
		}

		public Task<bool> ReadAsync(CancellationToken cancellationToken) =>
			ReadAsync(Command.Connection.AsyncIOBehavior, cancellationToken);

		public async Task<bool> ReadAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			m_row?.ClearData();
			m_row = m_readBuffer.Count > 0
				? m_readBuffer.Dequeue()
				: await ScanRowAsync(ioBehavior, m_row, cancellationToken).ConfigureAwait(false);

			if (m_row == null)
			{
				State = BufferState;
				return false;
			}
			State = ResultSetState.ReadingRows;
			return true;
		}

		public async Task<Row> BufferReadAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			m_rowBuffered?.BufferData();
			// ScanRowAsync sets m_rowBuffered to the next row if there is one
			if (await ScanRowAsync(ioBehavior, null, cancellationToken).ConfigureAwait(false) == null)
				return null;
			m_readBuffer.Enqueue(m_rowBuffered);
			return m_rowBuffered;
		}

		private ValueTask<Row> ScanRowAsync(IOBehavior ioBehavior, Row row, CancellationToken cancellationToken)
		{
			// if we've already read past the end of this resultset, Read returns false
			if (BufferState == ResultSetState.HasMoreData || BufferState == ResultSetState.NoMoreData || BufferState == ResultSetState.None)
				return new ValueTask<Row>((Row)null);

			var payloadTask = Session.ReceiveReplyAsync(ioBehavior, cancellationToken);
			return payloadTask.IsCompletedSuccessfully
				? new ValueTask<Row>(ScanRowAsyncRemainder(row, payloadTask.Result))
				: new ValueTask<Row>(ScanRowAsyncAwaited(row, payloadTask.AsTask()));
		}

		private async Task<Row> ScanRowAsyncAwaited(Row row, Task<PayloadData> payloadTask)
		{
			var payload = await payloadTask.ConfigureAwait(false);
			return ScanRowAsyncRemainder(row, payload);
		}

		private Row ScanRowAsyncRemainder(Row row, PayloadData payload)
		{
			if (EofPayload.IsEof(payload))
			{
				var eof = EofPayload.Create(payload);
				BufferState = (eof.ServerStatus & ServerStatus.MoreResultsExist) == 0 ? ResultSetState.NoMoreData : ResultSetState.HasMoreData;
				m_rowBuffered = null;
				return null;
			}

			var reader = new ByteArrayReader(payload.ArraySegment);
			for (var column = 0; column < m_dataOffsets.Length; column++)
			{
				var length = checked((int) ReadFieldLength(reader));
				m_dataLengths[column] = length == -1 ? 0 : length;
				m_dataOffsets[column] = length == -1 ? -1 : reader.Offset;
				reader.Offset += m_dataLengths[column];
			}

			if (row == null)
				row = new Row(this);
			row.SetData(m_dataLengths, m_dataOffsets, payload.ArraySegment.Array);
			m_rowBuffered = row;
			return row;
		}

		private static long ReadFieldLength(ByteArrayReader reader)
		{
			var leadByte = reader.ReadByte();
			switch (leadByte)
			{
				case 0xFB:
					return -1;
				case 0xFC:
					return reader.ReadFixedLengthUInt32(2);
				case 0xFD:
					return reader.ReadFixedLengthUInt32(3);
				case 0xFE:
					return checked((long) reader.ReadFixedLengthUInt64(8));
				default:
					return leadByte;
			}
		}

		public string GetName(int ordinal)
		{
			if (ColumnDefinitions == null)
				throw new IndexOutOfRangeException("There is no current result set.");
			if (ordinal < 0 || ordinal > ColumnDefinitions.Length)
				throw new IndexOutOfRangeException("value must be between 0 and {0}".FormatInvariant(ColumnDefinitions.Length - 1));
			return ColumnDefinitions[ordinal].Name;
		}

		public string GetDataTypeName(int ordinal)
		{
			if (ordinal < 0 || ordinal > ColumnDefinitions.Length)
				throw new ArgumentOutOfRangeException(nameof(ordinal), "value must be between 0 and {0}.".FormatInvariant(ColumnDefinitions.Length));

			var columnDefinition = ColumnDefinitions[ordinal];
			switch (columnDefinition.ColumnType)
			{
				case ColumnType.Tiny:
					return columnDefinition.ColumnLength == 1 ? "BOOL" : "TINYINT";

				case ColumnType.Short:
					return "SMALLINT";

				case ColumnType.Int24:
					return "MEDIUMINT";

				case ColumnType.Long:
					return "INT";

				case ColumnType.Longlong:
					return "BIGINT";

				case ColumnType.Bit:
					return "BIT";

				case ColumnType.String:
					return columnDefinition.CharacterSet == CharacterSet.Binary ? "BLOB" :
						(columnDefinition.ColumnFlags & ColumnFlags.Enum) != 0 ? "ENUM" :
						(columnDefinition.ColumnFlags & ColumnFlags.Set) != 0 ? "SET" :
						string.Format(CultureInfo.InvariantCulture, "CHAR({0})", columnDefinition.ColumnLength / SerializationUtility.GetBytesPerCharacter(columnDefinition.CharacterSet));

				case ColumnType.VarString:
				case ColumnType.TinyBlob:
				case ColumnType.Blob:
				case ColumnType.MediumBlob:
				case ColumnType.LongBlob:
					return columnDefinition.CharacterSet == CharacterSet.Binary ? "BLOB" : "VARCHAR";

				case ColumnType.Date:
					return "DATE";

				case ColumnType.DateTime:
					return "DATETIME";

				case ColumnType.Timestamp:
					return "TIMESTAMP";

				case ColumnType.Time:
					return "TIME";

				case ColumnType.Year:
					return "YEAR";

				case ColumnType.Float:
					return "FLOAT";

				case ColumnType.Double:
					return "DOUBLE";

				case ColumnType.Decimal:
				case ColumnType.NewDecimal:
					return "DECIMAL";

				case ColumnType.Json:
					return "JSON";

				default:
					throw new NotImplementedException("GetDataTypeName for {0} is not implemented".FormatInvariant(columnDefinition.ColumnType));
			}
		}

		public Type GetFieldType(int ordinal)
		{
			if (ordinal < 0 || ordinal > ColumnDefinitions.Length)
				throw new ArgumentOutOfRangeException(nameof(ordinal), "value must be between 0 and {0}.".FormatInvariant(ColumnDefinitions.Length));

			var columnDefinition = ColumnDefinitions[ordinal];
			var isUnsigned = (columnDefinition.ColumnFlags & ColumnFlags.Unsigned) != 0;
			switch (columnDefinition.ColumnType)
			{
				case ColumnType.Tiny:
					return columnDefinition.ColumnLength == 1 ? typeof(bool) :
						isUnsigned ? typeof(byte) : typeof(sbyte);

				case ColumnType.Int24:
				case ColumnType.Long:
					return isUnsigned ? typeof(uint) : typeof(int);

				case ColumnType.Longlong:
					return isUnsigned ? typeof(ulong) : typeof(long);

				case ColumnType.Bit:
					return typeof(ulong);

				case ColumnType.String:
					if (!Connection.OldGuids && columnDefinition.ColumnLength / SerializationUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 36)
						return typeof(Guid);
					goto case ColumnType.VarString;

				case ColumnType.VarString:
				case ColumnType.TinyBlob:
				case ColumnType.Blob:
				case ColumnType.MediumBlob:
				case ColumnType.LongBlob:
					return columnDefinition.CharacterSet == CharacterSet.Binary ?
						(Connection.OldGuids && columnDefinition.ColumnLength == 16 ? typeof(Guid) : typeof(byte[])) :
						typeof(string);

				case ColumnType.Json:
					return typeof(string);

				case ColumnType.Short:
					return isUnsigned ? typeof(ushort) : typeof(short);

				case ColumnType.Date:
				case ColumnType.DateTime:
				case ColumnType.Timestamp:
					return typeof(DateTime);

				case ColumnType.Time:
					return typeof(TimeSpan);

				case ColumnType.Year:
					return typeof(int);

				case ColumnType.Float:
					return typeof(float);

				case ColumnType.Double:
					return typeof(double);

				case ColumnType.Decimal:
				case ColumnType.NewDecimal:
					return typeof(decimal);

				default:
					throw new NotImplementedException("GetFieldType for {0} is not implemented".FormatInvariant(columnDefinition.ColumnType));
			}
		}

		public int FieldCount => ColumnDefinitions?.Length ?? 0;

		public bool HasRows
		{
			get
			{
				if (BufferState == ResultSetState.ReadResultSetHeader)
					return BufferReadAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult() != null;
				return BufferState == ResultSetState.ReadingRows;
			}
		}

		public Row GetCurrentRow()
		{
			if (State != ResultSetState.ReadingRows)
				throw new InvalidOperationException("Read must be called first.");
			if (m_row == null)
				throw new InvalidOperationException("There is no current row.");
			return m_row;
		}

		public readonly MySqlDataReader DataReader;
		public MySqlCommand Command => DataReader.Command;
		public MySqlConnection Connection => DataReader.Connection;
		public MySqlSession Session => DataReader.Session;

		public ResultSetState BufferState { get; private set; }
		public ColumnDefinitionPayload[] ColumnDefinitions { get; private set; }
		public long LastInsertId { get; private set; }
		public int RecordsAffected { get; private set; }
		public ResultSetState State { get; private set; }

		int[] m_dataLengths;
		int[] m_dataOffsets;
		readonly Queue<Row> m_readBuffer = new Queue<Row>();
		Row m_row;
		Row m_rowBuffered;
	}
}
