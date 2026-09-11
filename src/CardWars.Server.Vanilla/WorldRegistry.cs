using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Server.Vanilla;

public class WorldRegistry
{
	public Registry<ResourceId, CompoundTag> Templates { get; } = new();
	public ResourceId DefaultWorld { get; set; }
}
