using CardWars.BattleEngine;
using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class BattleInstance : ServerInstance
{
	[DataTag] public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	public BattleEngine.BattleEngine? Engine { get; set; }

	public override void HandlePacket(PlayerSession session, IPacket packet) { }

	public override void Tick(float deltaTime) { }
}
