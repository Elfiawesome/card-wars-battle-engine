using CardWars.Core.Data;
using CardWars.Core.Registry;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public sealed class WorldInstanceProvider : IInstanceProvider
{
	private readonly WorldRegistry _worldRegistry;

	public WorldInstanceProvider(WorldRegistry worldRegistry)
	{
		_worldRegistry = worldRegistry;
	}

	public IServerInstance Create(InstanceCreateContext ctx)
	{
		var worldId = ResourceId.FromFlatString(ctx.Key);
		var template = _worldRegistry.Templates.Get(worldId) ?? new CompoundTag();
		return new WorldInstance
		{
			InstanceId = Guid.NewGuid(),
			WorldId = worldId,
			TemplateData = template,
		};
	}

	public IServerInstance Load(InstanceLoadContext ctx)
	{
		var instance = DataTagMapper.FromTag<WorldInstance>(ctx.SavedData);
		instance.TemplateData = _worldRegistry.Templates.Get(instance.WorldId) ?? new CompoundTag();
		return instance;
	}
}
