using CardWars.Core.Data;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class WorldInstanceProvider(WorldRegistry worlds, SessionStorage session)
	: ServerInstanceProvider<WorldInstance, ResourceId>
{
	protected override WorldInstance Create(ResourceId worldId)
	{
		var template = worlds.Templates.Get(worldId);
		var savedData = session.LoadInstance(worldId.ToFlatString()) as CompoundTag;
		return new WorldInstance
		{
			InstanceId = Guid.NewGuid(),
			WorldId = worldId,
			TemplateData = template ?? new CompoundTag(),
			Data = savedData ?? new CompoundTag()
		};
	}

	protected override void Save(WorldInstance instance)
		=> session.SaveInstance(instance.WorldId.ToFlatString(), DataTagMapper.ToTag(instance, false));
}
