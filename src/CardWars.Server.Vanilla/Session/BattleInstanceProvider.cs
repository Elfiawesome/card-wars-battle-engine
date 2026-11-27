using CardWars.BattleEngine.registry;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class BattleInstanceProvider(
	SessionStorage session,
	BattleEngineRegistry battleEngineRegistry,
	ResourceId providerId) : ServerInstanceProvider<BattleInstance>
{
	public override BattleInstance Create(string saveId)
	{
		BattleInstance instance;

		if (session.LoadInstance(saveId) is CompoundTag saveTag)
		{
			instance = session.DataTags.FromTag<BattleInstance>(saveTag);
			instance.InstanceId = Guid.Parse(saveId);
			instance.InstanceProviderId = providerId;
		}
		else
		{
			instance = new BattleInstance
			{
				InstanceId = Guid.Parse(saveId),
				InstanceProviderId = providerId,
			};
		}

		instance.InitializeEngine(new BattleEngine.BattleEngine(
			battleEngineRegistry,
			instance.BroadcastBatch));
		return instance;
	}

	public override string? Save(BattleInstance serverInstance, StoragePath instanceStoragePath)
	{
		var saveId = serverInstance.InstanceId.ToString();

		if (serverInstance.Players.Count == 0)
		{
			session.InstancesDir.Combine(saveId).WithExtension("json").Delete();
			return null;
		}

		serverInstance.Snapshot = serverInstance.Engine?.State.ToSnapshot(serverInstance.Engine.Registry.DataTags);
		session.SaveInstance(saveId, session.DataTags.ToTag(serverInstance, false));
		return saveId;
	}
}
