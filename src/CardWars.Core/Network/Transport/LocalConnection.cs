using System.Collections.Concurrent;
using CardWars.Core.Network.Packet;

namespace CardWars.Core.Network.Transport;

public class LocalConnection : IConnection
{
	private readonly ConcurrentQueue<IPacket> _receiveQueue = new();

	public LocalConnection? Peer { get; private set; }
	public bool IsConnected { get; private set; } = true;

	public static (LocalConnection client, LocalConnection server) CreatePair()
	{
		var c = new LocalConnection();
		var s = new LocalConnection();
		c.Peer = s;
		s.Peer = c;
		return (c, s);
	}

	public void Send(IPacket packet)
	{
		if (!IsConnected || Peer == null) return;
		Peer._receiveQueue.Enqueue(packet);
	}

	public bool TryReceive(out IPacket? packet)
		=> _receiveQueue.TryDequeue(out packet);

	public void Disconnect()
	{
		IsConnected = false;
		if (Peer != null) Peer.IsConnected = false;
	}

	public void Dispose() => Disconnect();
}