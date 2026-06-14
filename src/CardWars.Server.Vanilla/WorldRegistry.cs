using System;
using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.Server.Vanilla;

public class WorldRegistry
{
	public Registry<ResourceId, CompoundTag> Templates { get; } = new();
	public ResourceId DefaultWorld { get; set; }

	public WorldInstance CreateWorld(ResourceId worldId)
	{
		var template = Templates.Get(worldId)
			?? throw new InvalidOperationException($"Unknown world template: {worldId}");

		var instance = new WorldInstance(Guid.NewGuid());
		instance.Load(template);
		return instance;
	}
}
