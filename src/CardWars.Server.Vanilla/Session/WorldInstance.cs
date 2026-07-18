using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class WorldInstance : IServerInstance
{
	[DataTag] public Guid InstanceId { get; set; }
	[DataTag] public ResourceId WorldId { get; set; }
	[DataTag] public CompoundTag Data { get; set; } = new(); // session related data
	public CompoundTag TemplateData { get; set; } = new(); // For reference only

	public void AddPlayer(PlayerSession player) { }
	public void RemovePlayer(PlayerSession player) { }

	public void HandlePacket(PlayerSession session, IPacket packet) { }
	public void Tick(float deltaTime) { }
}
