using CardWars.Core.Network.Transport;

namespace CardWars.Server.Listener;

public interface IListener
{
	public Action<IConnection>? OnNewConnection { get; set; }
	public void Start();
	public void Stop();
}