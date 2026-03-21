using CardWars.Core.Network.Packet;

namespace CardWars.Server.Session;

public interface IServerInstance
{
	Guid InstanceId { get; }
	
	public void AddPlayer(PlayerSession player);
	public void RemovePlayer(PlayerSession player);

	void HandlePacket(PlayerSession session, IPacket packet);
	void Tick(float deltaTime);
};