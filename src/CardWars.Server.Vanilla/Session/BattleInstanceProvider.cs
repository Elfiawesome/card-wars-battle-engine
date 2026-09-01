using CardWars.BattleEngine;
using CardWars.Core.Data;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class BattleInstanceProvider(
	SessionStorage session,
	BattleEngineRegistry sharedRegistry,
	ResourceId providerId) : ServerInstanceProvider<BattleInstance>
{
	public override BattleInstance Create(string saveId)
	{
		var instance = new BattleInstance
		{
			InstanceId = Guid.Parse(saveId),
			InstanceProviderId = providerId,
		};
		instance.Engine = new BattleEngine.BattleEngine { Registry = sharedRegistry };
		return instance;
	}

	public override string? Save(BattleInstance serverInstance, StoragePath instanceStoragePath)
	{
		var saveId = serverInstance.InstanceId.ToString();

		// TODO: serialize battle state (GameState) — for now persist identity only.
		session.SaveInstance(saveId, DataTagMapper.ToTag(serverInstance, false));
		return saveId;
	}
}
