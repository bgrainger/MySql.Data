﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.Protocol.Serialization;
using MySql.Data.Serialization;

namespace MySql.Data.MySqlClient
{
	internal sealed class ConnectionPool
	{
		public async Task<MySqlSession> GetSessionAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// if all sessions are used, see if any have been leaked and can be recovered
			// check at most once per second (although this isn't enforced via a mutex so multiple threads might block
			// on the lock in RecoverLeakedSessions in high-concurrency situations
			if (m_sessionSemaphore.CurrentCount == 0 && unchecked(((uint) Environment.TickCount) - m_lastRecoveryTime) >= 1000u)
				RecoverLeakedSessions();

			// wait for an open slot (until the cancellationToken is cancelled, which is typically due to timeout)
			if (ioBehavior == IOBehavior.Asynchronous)
				await m_sessionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
			else
				m_sessionSemaphore.Wait(cancellationToken);

			try
			{
				// check for a waiting session
				MySqlSession session = null;
				lock (m_sessions)
				{
					if (m_sessions.Count > 0)
					{
						session = m_sessions.First.Value;
						m_sessions.RemoveFirst();
					}
				}
				if (session != null)
				{
					if (session.PoolGeneration != m_generation || !await session.TryPingAsync(ioBehavior, cancellationToken).ConfigureAwait(false))
					{
						// session is either old or cannot communicate with the server
						await session.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
					}
					else
					{
						// pooled session is ready to be used; return it
						lock (m_leasedSessions)
							m_leasedSessions.Add(session.Id, new WeakReference<MySqlSession>(session));
						return session;
					}
				}

				// create a new session
				session = new MySqlSession(this, m_generation, Interlocked.Increment(ref m_lastId));
				await session.ConnectAsync(m_connectionSettings, ioBehavior, cancellationToken).ConfigureAwait(false);
				lock (m_leasedSessions)
					m_leasedSessions.Add(session.Id, new WeakReference<MySqlSession>(session));
				return session;
			}
			catch
			{
				m_sessionSemaphore.Release();
				throw;
			}
		}

		private bool SessionIsHealthy(MySqlSession session)
		{
			if (!session.IsConnected)
				return false;
			if (session.PoolGeneration != m_generation)
				return false;
			if (session.DatabaseOverride != null)
				return false;
			if (m_connectionSettings.ConnectionLifeTime > 0
			    && (DateTime.UtcNow - session.CreatedUtc).TotalSeconds >= m_connectionSettings.ConnectionLifeTime)
				return false;

			return true;
		}

		public void Return(MySqlSession session)
		{
			// NOTE: deliberately calling 'async void' method so that first part runs synchronously, but remainder gets queued
			// to an I/O completion thread to run in the background
			ReturnAsync(session);
		}

		private async void ReturnAsync(MySqlSession session)
		{
			var succeeded = false;
			try
			{
				try
				{
					lock (m_leasedSessions)
						m_leasedSessions.Remove(session.Id);

					if (SessionIsHealthy(session))
					{
						if (m_connectionSettings.ConnectionReset)
							await session.ResetConnectionAsync(m_connectionSettings, IOBehavior.Asynchronous, CancellationToken.None).ConfigureAwait(false);

						lock (m_sessions)
							m_sessions.AddFirst(session);
						succeeded = true;
					}
				}
				catch (Exception)
				{
					// session will be disposed below
				}

				if (!succeeded)
				{
					// clean up the session, ignoring any errors
					try
					{
						await session.DisposeAsync(IOBehavior.Asynchronous, CancellationToken.None).ConfigureAwait(false);
					}
					catch (Exception)
					{
					}
				}
			}
			finally
			{
				m_sessionSemaphore.Release();
			}
		}

		public async Task ClearAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			// increment the generation of the connection pool
			Interlocked.Increment(ref m_generation);
			await CleanPoolAsync(ioBehavior, session => session.PoolGeneration != m_generation, false, cancellationToken).ConfigureAwait(false);
		}

		public async Task ReapAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			RecoverLeakedSessions();
			if (m_connectionSettings.ConnectionIdleTimeout == 0)
				return;
			await CleanPoolAsync(ioBehavior, session => (DateTime.UtcNow - session.LastReturnedUtc).TotalSeconds >= m_connectionSettings.ConnectionIdleTimeout, true, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Examines all the <see cref="WeakReference{MySqlSession}"/> in <see cref="m_leasedSessions"/> to determine if any
		/// <see cref="MySqlSession"/> objects have been garbage-collected. If so, assumes that the related <see cref="MySqlConnection"/>
		/// was not properly disposed but the associated server connection has been closed (by the finalizer). Releases the semaphore
		/// once for each leaked session to allow new client connections to be made.
		/// </summary>
		private void RecoverLeakedSessions()
		{
			var recoveredIds = new List<int>();
			lock (m_leasedSessions)
			{
				m_lastRecoveryTime = unchecked((uint) Environment.TickCount);
				foreach (var pair in m_leasedSessions)
				{
					if (!pair.Value.TryGetTarget(out var _))
						recoveredIds.Add(pair.Key);
				}
				foreach (var id in recoveredIds)
					m_leasedSessions.Remove(id);
			}
			if (recoveredIds.Count > 0)
				m_sessionSemaphore.Release(recoveredIds.Count);
		}

		private async Task CleanPoolAsync(IOBehavior ioBehavior, Func<MySqlSession, bool> shouldCleanFn, bool respectMinPoolSize, CancellationToken cancellationToken)
		{
			// synchronize access to this method as only one clean routine should be run at a time
			if (ioBehavior == IOBehavior.Asynchronous)
				await m_cleanSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
			else
				m_cleanSemaphore.Wait(cancellationToken);

			try
			{
				var waitTimeout = TimeSpan.FromMilliseconds(10);
				while (true)
				{
					// if respectMinPoolSize is true, return if (leased sessions + waiting sessions <= minPoolSize)
					if (respectMinPoolSize)
						lock (m_sessions)
							if (m_connectionSettings.MaximumPoolSize - m_sessionSemaphore.CurrentCount + m_sessions.Count <= m_connectionSettings.MinimumPoolSize)
								return;

					// try to get an open slot; if this fails, connection pool is full and sessions will be disposed when returned to pool
					if (ioBehavior == IOBehavior.Asynchronous)
					{
						if (!await m_sessionSemaphore.WaitAsync(waitTimeout, cancellationToken).ConfigureAwait(false))
							return;
					}
					else
					{
						if (!m_sessionSemaphore.Wait(waitTimeout, cancellationToken))
							return;
					}

					try
					{
						// check for a waiting session
						MySqlSession session = null;
						lock (m_sessions)
						{
							if (m_sessions.Count > 0)
							{
								session = m_sessions.Last.Value;
								m_sessions.RemoveLast();
							}
						}
						if (session == null)
							return;

						if (shouldCleanFn(session))
						{
							// session should be cleaned; dispose it and keep iterating
							await session.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
						}
						else
						{
							// session should not be cleaned; put it back in the queue and stop iterating
							lock (m_sessions)
								m_sessions.AddLast(session);
							return;
						}
					}
					finally
					{
						m_sessionSemaphore.Release();
					}
				}
			}
			finally
			{
				m_cleanSemaphore.Release();
			}
		}

		public static ConnectionPool GetPool(ConnectionSettings cs)
		{
			if (!cs.Pooling)
				return null;

			var key = cs.ConnectionString;

			if (!s_pools.TryGetValue(key, out var pool))
			{
				pool = s_pools.GetOrAdd(cs.ConnectionString, newKey => new ConnectionPool(cs));
			}
			return pool;
		}

		public static async Task ClearPoolsAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			foreach (var pool in s_pools.Values)
				await pool.ClearAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
		}

		public static async Task ReapPoolsAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
		{
			foreach (var pool in s_pools.Values)
				await pool.ReapAsync(ioBehavior, cancellationToken).ConfigureAwait(false);
		}

		private ConnectionPool(ConnectionSettings cs)
		{
			m_connectionSettings = cs;
			m_generation = 0;
			m_cleanSemaphore = new SemaphoreSlim(1);
			m_sessionSemaphore = new SemaphoreSlim(cs.MaximumPoolSize);
			m_sessions = new LinkedList<MySqlSession>();
			m_leasedSessions = new Dictionary<int, WeakReference<MySqlSession>>();
		}

		static readonly ConcurrentDictionary<string, ConnectionPool> s_pools = new ConcurrentDictionary<string, ConnectionPool>();
#if DEBUG
		static readonly TimeSpan ReaperInterval = TimeSpan.FromSeconds(1);
#else
		static readonly TimeSpan ReaperInterval = TimeSpan.FromMinutes(1);
#endif
		static readonly Task Reaper = Task.Run(async () => {
			while (true)
			{
				var task = Task.Delay(ReaperInterval);
				try
				{
					await ReapPoolsAsync(IOBehavior.Asynchronous, new CancellationTokenSource(ReaperInterval).Token).ConfigureAwait(false);
				}
				catch
				{
					// do nothing; we'll try to reap again
				}
				await task.ConfigureAwait(false);
			}
		});

		int m_generation;
		readonly SemaphoreSlim m_cleanSemaphore;
		readonly SemaphoreSlim m_sessionSemaphore;
		readonly LinkedList<MySqlSession> m_sessions;
		readonly ConnectionSettings m_connectionSettings;
		readonly Dictionary<int, WeakReference<MySqlSession>> m_leasedSessions;
		int m_lastId;
		uint m_lastRecoveryTime;
	}
}
