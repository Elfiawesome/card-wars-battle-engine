using System.Collections.Concurrent;
using CardWars.Core.Network.Packet;

namespace CardWars.Core.Network.Transport;

public class LocalSerializedConnection : IConnection
{
	private readonly ConcurrentQueue<string> _receiveQueue = new();

	public LocalSerializedConnection? Peer { get; private set; }
	public bool IsConnected { get; private set; } = true;

	public static (LocalSerializedConnection client, LocalSerializedConnection server) CreatePair()
	{
		var c = new LocalSerializedConnection();
		var s = new LocalSerializedConnection();
		c.Peer = s;
		s.Peer = c;
		return (c, s);
	}

	public void Send(IPacket packet)
	{
		if (!IsConnected || Peer == null) return;
		Peer._receiveQueue.Enqueue(PacketCodec.Encode(packet));
	}

	public bool TryReceive(out IPacket? packet)
	{
		if (_receiveQueue.TryDequeue(out var json))
		{
			packet = PacketCodec.Decode(json);
			return packet != null;
		}
		packet = null;
		return false;
	}

	public void Disconnect()
	{
		IsConnected = false;
		if (Peer != null) Peer.IsConnected = false;
	}

	public void Dispose() => Disconnect();
}