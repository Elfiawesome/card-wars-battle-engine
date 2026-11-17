using CardWars.BattleEngine;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class BattleInstanceProvider(
	SessionStorage session,
	BattleEngineRegistry sharedRegistry,
	ResourceId providerId,
	ILogger? logger = null) : ServerInstanceProvider<BattleInstance>
{
	private readonly ILogger _logger = logger ?? NullLogger.Instance;

	public override BattleInstance Create(string saveId)
	{
		var instance = new BattleInstance
		{
			InstanceId = Guid.Parse(saveId),
			InstanceProviderId = providerId,
		};
		instance.Engine = new BattleEngine.BattleEngine
		{
			Registry = sharedRegistry,
			// Thread the server's owned logger into each battle engine so block-execution DEBUG,
			// GameState.Require/script WARNs and stack-limit ERRORs reach the server console as the
			// old static Log did (behavior preservation); NullLogger only where no owner exists.
			Logger = _logger,
			OnBlockBatchEvent = instance.BroadcastBatch
		};
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
