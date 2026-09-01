using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class WorldInstanceProvider(
	WorldRegistry worlds,
	SessionStorage session,
	ResourceId providerId) : ServerInstanceProvider<WorldInstance>
{
	public override WorldInstance Create(string saveId)
	{
		var worldId = ResourceId.Parse(saveId);

		WorldInstance instance;
		var save = session.LoadInstance(worldId.ToFlatString());
		if (save is CompoundTag saveTag)
		{
			instance = DataTagMapper.FromTag<WorldInstance>(saveTag);
		}
		else
		{
			instance = new WorldInstance
			{
				InstanceId = Guid.NewGuid(),
				InstanceProviderId = providerId,
				WorldId = worldId,
			};
		}

		instance.TemplateData = worlds.Templates.Get(worldId) is { } template
			? (CompoundTag)template.Clone()
			: new CompoundTag();

		if (instance.TemplateData.Count == 0)
			Logger.Warn($"No world template found for '{worldId}'. World instance will have no template data.");

		return instance;
	}

	public override string? Save(WorldInstance serverInstance, StoragePath instanceStoragePath)
	{
		session.SaveInstance(serverInstance.WorldId.ToFlatString(), DataTagMapper.ToTag(serverInstance, false));
		return serverInstance.WorldId.ToString();
	}
}
