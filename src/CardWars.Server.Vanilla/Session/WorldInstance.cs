using CardWars.Core.Network.Packet;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class WorldInstance(Guid id) : IServerInstance
{
	public Guid InstanceId => id;

	public void AddPlayer(PlayerSession player) { }
	public void RemovePlayer(PlayerSession player) { }

	public void HandlePacket(PlayerSession session, IPacket packet) { }
	public void Tick(float deltaTime) { }
}
