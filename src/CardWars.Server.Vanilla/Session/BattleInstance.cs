using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class BattleInstance(ResourceId providerId) : IServerInstance
{
	[DataTag] public Guid InstanceId { get; set; }
	public ResourceId InstanceProviderId => providerId;

	public void AddPlayer(PlayerSession player) { }
	public void RemovePlayer(PlayerSession player) { }

	public void HandlePacket(PlayerSession session, IPacket packet) { }
	public void Tick(float deltaTime) { }
}

