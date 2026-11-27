using System.Collections.Concurrent;
using CardWars.Core.Network.Packet;

namespace CardWars.Core.Network.Transport;

public class LocalSerializedConnection : IConnection
{
	private readonly ConcurrentQueue<string> _receiveQueue = new();
	private readonly PacketCodec _codec;

	public LocalSerializedConnection(PacketCodec codec) => _codec = codec;

	public LocalSerializedConnection? Peer { get; private set; }
	public bool IsConnected { get; private set; } = true;

	public static (LocalSerializedConnection client, LocalSerializedConnection server) CreatePair(PacketCodec codec)
	{
		var c = new LocalSerializedConnection(codec);
		var s = new LocalSerializedConnection(codec);
		c.Peer = s;
		s.Peer = c;
		return (c, s);
	}

	public void Send(IPacket packet)
	{
		if (!IsConnected || Peer is null) return;
		Peer._receiveQueue.Enqueue(_codec.Encode(packet));
	}

	public bool TryReceive(out IPacket? packet)
	{
		if (_receiveQueue.TryDequeue(out var json))
		{
			packet = _codec.Decode(json);
			return packet is not null;
		}
		packet = null;
		return false;
	}

	public void Disconnect()
	{
		IsConnected = false;
		if (Peer is not null) Peer.IsConnected = false;
	}

	public void Dispose() => Disconnect();
}
