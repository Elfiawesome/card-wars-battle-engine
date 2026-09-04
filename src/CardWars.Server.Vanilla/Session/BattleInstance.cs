using CardWars.BattleEngine;
using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class BattleInstance : ServerInstance
{
	[DataTag] public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	public BattleEngine.BattleEngine? Engine { get; set; }

	public override void HandlePacket(PacketContextServer context, IPacket packet) { }

	public override void Tick(float deltaTime) { }
}
