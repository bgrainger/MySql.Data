using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MySql.Data.Protocol.Serialization
{
	internal sealed class StreamByteHandler : IByteHandler
	{
		public StreamByteHandler(Stream stream)
		{
			m_stream = stream;
			m_closeStream = m_stream.Dispose;
			RemainingTimeout = Constants.InfiniteTimeout;
		}

		public void Dispose() => m_stream.Dispose();

		public int RemainingTimeout { get; set; }

		public ValueTask<int> ReadBytesAsync(ArraySegment<byte> buffer, IOBehavior ioBehavior)
		{
			return (ioBehavior == IOBehavior.Asynchronous) ?
				new ValueTask<int>(DoReadBytesAsync(buffer)) : DoReadBytesSync(buffer);

			ValueTask<int> DoReadBytesSync(ArraySegment<byte> buffer_)
			{
				if (RemainingTimeout <= 0)
					return ValueTaskExtensions.FromException<int>(MySqlException.CreateForTimeout());

				m_stream.ReadTimeout = RemainingTimeout == Constants.InfiniteTimeout ? Timeout.Infinite : RemainingTimeout;
				var startTime = RemainingTimeout == Constants.InfiniteTimeout ? 0 : Environment.TickCount;
				int bytesRead;
				try
				{
					bytesRead = m_stream.Read(buffer_.Array, buffer_.Offset, buffer_.Count);
				}
				catch (Exception ex)
				{
					if (ex is IOException && RemainingTimeout != Constants.InfiniteTimeout)
						return ValueTaskExtensions.FromException<int>(MySqlException.CreateForTimeout(ex));
					return ValueTaskExtensions.FromException<int>(ex);
				}
				if (RemainingTimeout != Constants.InfiniteTimeout)
					RemainingTimeout -= unchecked(Environment.TickCount - startTime);
				return new ValueTask<int>(bytesRead);
			}

			async Task<int> DoReadBytesAsync(ArraySegment<byte> buffer_)
			{
				var startTime = RemainingTimeout == Constants.InfiniteTimeout ? 0 : Environment.TickCount;
				var timerId = RemainingTimeout == Constants.InfiniteTimeout ? 0 : TimerQueue.Instance.Add(RemainingTimeout, m_closeStream);
				int bytesRead;
				try
				{
					bytesRead = await m_stream.ReadAsync(buffer_.Array, buffer_.Offset, buffer_.Count).ConfigureAwait(false);
				}
				catch (ObjectDisposedException ex)
				{
					if (RemainingTimeout != Constants.InfiniteTimeout)
					{
						RemainingTimeout -= unchecked(Environment.TickCount - startTime);
						if (!TimerQueue.Instance.Remove(timerId))
							throw MySqlException.CreateForTimeout(ex);
					}
					throw;
				}
				if (RemainingTimeout != Constants.InfiniteTimeout)
				{
					RemainingTimeout -= unchecked(Environment.TickCount - startTime);
					if (!TimerQueue.Instance.Remove(timerId))
						throw MySqlException.CreateForTimeout();
				}
				return bytesRead;
			}
		}

		public ValueTask<int> WriteBytesAsync(ArraySegment<byte> data, IOBehavior ioBehavior)
		{
			if (ioBehavior == IOBehavior.Asynchronous)
			{
				return new ValueTask<int>(DoWriteBytesAsync(data));
			}
			else
			{
				m_stream.Write(data.Array, data.Offset, data.Count);
				return default(ValueTask<int>);
			}

			async Task<int> DoWriteBytesAsync(ArraySegment<byte> data_)
			{
				await m_stream.WriteAsync(data_.Array, data_.Offset, data_.Count).ConfigureAwait(false);
				return 0;
			}
		}

		readonly Stream m_stream;
		readonly Action m_closeStream;
	}
}
