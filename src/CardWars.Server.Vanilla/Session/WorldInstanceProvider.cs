using CardWars.Core.Data;
using CardWars.Core.Logging;
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
		var rawData = session.LoadInstance(worldId.ToFlatString());
		var savedData = rawData is CompoundTag c ? c : null;
		if (rawData != null && savedData == null)
			Logger.Warn($"WorldInstance '{worldId}' has corrupted save data. loading defaults.");

		return new WorldInstance
		{
			InstanceId = Guid.NewGuid(),
			WorldId = worldId,
			TemplateData = template ?? new CompoundTag(),
			Data = savedData ?? new CompoundTag()
		};
	}

	protected override WorldInstance Deserialize(CompoundTag data, ResourceId id)
		=> DataTagMapper.FromTag<WorldInstance>(data);

	protected override CompoundTag Serialize(WorldInstance instance)
		=> DataTagMapper.ToTag(instance, false);

	protected override ResourceId Parse(string id) => ResourceId.Parse(id);
}
