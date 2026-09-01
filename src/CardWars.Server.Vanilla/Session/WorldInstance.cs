using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class WorldInstance : ServerInstance
{
	[DataTag] public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	[DataTag] public ResourceId WorldId { get; set; }
	[DataTag] public CompoundTag Data { get; set; } = new(); // session related data
	public CompoundTag TemplateData { get; set; } = new(); // For reference only

	public override void HandlePacket(PlayerSession session, IPacket packet) { }

	public override void Tick(float deltaTime) { }
}
