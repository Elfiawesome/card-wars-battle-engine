using CardWars.Server.Packet;

namespace CardWars.Server.Transport;

public interface IConnection : IDisposable
{
	public bool IsConnected { get; }
	public void Send(IPacket packet);
	public bool TryReceive(out IPacket? packet);
	public void Disconnect();
}
