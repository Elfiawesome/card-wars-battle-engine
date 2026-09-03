using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.View;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class WorldInstance : ServerInstance
{
	public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	[DataTag] public ResourceId WorldId { get; set; }
	[DataTag] public int DebugLifespan { get; set; } = 0;
	[DataTag] public CompoundTag Data { get; set; } = new(); // session related data
	public CompoundTag TemplateData { get; set; } = new(); // For reference only

	public override void HandlePacket(PlayerSession session, IPacket packet) { }

	public override void Tick(float deltaTime)
	{
		DebugLifespan++;
	}

	public WorldView GetWorldView()
	{
		return new()
		{
			Players = [.. Players.Select(p => new PlayerView() { Id = p.PlayerId })]
		};
	}
}
