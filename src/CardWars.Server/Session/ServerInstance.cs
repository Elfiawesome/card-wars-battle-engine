using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;

namespace CardWars.Server.Session;

public interface IServerInstance
{
	Guid InstanceId { get; }
	ResourceId InstanceProviderId { get; }

	public void AddPlayer(PlayerSession player);
	public void RemovePlayer(PlayerSession player);

	void HandlePacket(PlayerSession session, IPacket packet);
	void Tick(float deltaTime);
};