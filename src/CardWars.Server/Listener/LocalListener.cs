using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;

namespace CardWars.Server.Listener;

public class LocalListener(PacketCodec codec) : IListener
{
	public Action<IConnection>? OnNewConnection { get; set; }
	public bool IsListening { get; private set; }
	public bool IsSerialized { get; set; } = false;

	public void Start() => IsListening = true;
	public void Stop() => IsListening = false;

	public IConnection ConnectClient()
	{
		if (!IsListening)
			throw new InvalidOperationException("Server is not listening.");

		(IConnection, IConnection) pair = IsSerialized
			? LocalSerializedConnection.CreatePair(codec)
			: LocalConnection.CreatePair();
		var clientConn = pair.Item1;
		var serverConn = pair.Item2;

		OnNewConnection?.Invoke(serverConn);

		return clientConn;
	}
}