using CardWars.Server.Transport;

namespace CardWars.Server.Listener;

public class TcpGameListener : IListener
{
	public Action<IConnection>? OnNewConnection { get; set; }

	public void Start()
	{
		
	}

	public void Stop()
	{
		
	}
}