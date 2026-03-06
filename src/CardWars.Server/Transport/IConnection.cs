using CardWars.Server.Packet;

namespace CardWars.Server.Transport;

public interface IConnection : IDisposable
{
	public string Id { get; }
	public bool IsConnected { get; }
	public void Send(IPacket packet);
	public bool TryReceive(IPacket packet);
	public void Disconnect();
}
