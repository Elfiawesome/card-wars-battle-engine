using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;

namespace CardWars.Core.Network.Transport;

public class TcpConnection : IConnection
{
	private readonly TcpClient _tcpClient;
	private readonly NetworkStream _stream;
	private readonly ConcurrentQueue<IPacket> _receiveQueue = new();
	private readonly CancellationTokenSource _cts = new();

	public bool IsConnected => _tcpClient.Connected;

	static TcpConnection()
	{
		DataTagTypeRegistry.ScanAssembly(typeof(IPacket).Assembly);
	}

	public TcpConnection(TcpClient tcpClient)
	{
		_tcpClient = tcpClient;
		_stream = tcpClient.GetStream();
		Task.Run(() => ReceiveLoop(_cts.Token));
	}

	public void Send(IPacket packet)
	{
		if (!IsConnected) return;

		try
		{
			var tag = DataTagMapper.ToTag(packet);
			var json = DataTagSerializer.Serialize(tag);
			var bytes = Encoding.UTF8.GetBytes(json);
			var length = BitConverter.GetBytes(bytes.Length);

			lock (_stream)
			{
				_stream.Write(length, 0, length.Length);
				_stream.Write(bytes, 0, bytes.Length);
				_stream.Flush();
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"TcpConnection: Send failed: {ex.Message}");
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
				var tag = DataTagSerializer.Deserialize<CompoundTag>(json);
				if (tag == null) continue;

				var packet = DataTagMapper.FromTag<IPacket>(tag);
				if (packet != null)
					_receiveQueue.Enqueue(packet);
			}
			catch (ObjectDisposedException) { break; }
			catch (IOException) { break; }
			catch when (token.IsCancellationRequested) { break; }
			catch (Exception ex)
			{
				Logger.Error($"TcpConnection: Receive failed: {ex.Message}");
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
