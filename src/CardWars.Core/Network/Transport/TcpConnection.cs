using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;

namespace CardWars.Core.Network.Transport;

public class TcpConnection : IConnection
{
	private readonly TcpClient _tcpClient;
	private readonly NetworkStream _stream;
	private readonly ConcurrentQueue<IPacket> _receiveQueue = new();
	private readonly CancellationTokenSource _cts = new();
	private readonly PacketCodec _codec;

	public bool IsConnected => _tcpClient.Connected;

	public TcpConnection(TcpClient tcpClient, PacketCodec codec)
	{
		_tcpClient = tcpClient;
		_tcpClient.NoDelay = true;
		_stream = tcpClient.GetStream();
		_codec = codec;
		Task.Run(() => ReceiveLoop(_cts.Token));
	}

	public void Send(IPacket packet)
	{
		if (!IsConnected) return;

		try
		{
			var json = _codec.Encode(packet);
			var payload = Encoding.UTF8.GetBytes(json);

			var buffer = new byte[4 + payload.Length];
			BitConverter.TryWriteBytes(buffer.AsSpan(0, 4), payload.Length);
			payload.CopyTo(buffer, 4);

			lock (_stream)
			{
				_stream.Write(buffer, 0, buffer.Length);
				_stream.Flush();
			}
		}
		catch (Exception ex)
		{
			Log.Error($"TcpConnection: Send failed: {ex.Message}");
			Disconnect();
		}
	}

	public bool TryReceive(out IPacket? packet)
		=> _receiveQueue.TryDequeue(out packet);

	public void Disconnect()
	{
		_cts.Cancel();
		try { _stream.Close(); } catch { }
		try { _tcpClient.Close(); } catch { }
	}

	public void Dispose() => Disconnect();

	private void ReceiveLoop(CancellationToken token)
	{
		var lengthBuffer = new byte[4];

		while (!token.IsCancellationRequested)
		{
			try
			{
				if (!ReadExact(lengthBuffer, 0, 4, token)) break;

				var payloadLength = BitConverter.ToInt32(lengthBuffer, 0);
				if (payloadLength <= 0 || payloadLength > 1024 * 1024) break;

				var payloadBuffer = new byte[payloadLength];
				if (!ReadExact(payloadBuffer, 0, payloadLength, token)) break;

				var json = Encoding.UTF8.GetString(payloadBuffer);
				var packet = _codec.Decode(json);
				if (packet is not null)
					_receiveQueue.Enqueue(packet);
			}
			catch (ObjectDisposedException) { break; }
			catch (IOException) { break; }
			catch when (token.IsCancellationRequested) { break; }
			catch (Exception ex)
			{
				Log.Error($"TcpConnection: Receive failed: {ex.Message}");
			}
		}

		Disconnect();
	}

	private bool ReadExact(byte[] buffer, int offset, int count, CancellationToken token)
	{
		var remaining = count;
		while (remaining > 0)
		{
			var read = _stream.Read(buffer, offset + count - remaining, remaining);
			if (read == 0) return false;
			remaining -= read;
		}
		return true;
	}
}
