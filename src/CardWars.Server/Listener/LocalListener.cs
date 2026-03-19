using CardWars.Core.Network.Transport;

namespace CardWars.Server.Listener;

public class LocalListener : IListener
{
	public Action<IConnection>? OnNewConnection { get; set; }
	public bool IsListening { get; private set; }

	public void Start() => IsListening = true;
	public void Stop() => IsListening = false;

	public LocalConnection ConnectClient()
	{
		if (!IsListening)
			throw new InvalidOperationException("Server is not listening.");

		var (clientConn, serverConn) = LocalConnection.CreatePair();

		OnNewConnection?.Invoke(serverConn);

		return clientConn;
	}
}