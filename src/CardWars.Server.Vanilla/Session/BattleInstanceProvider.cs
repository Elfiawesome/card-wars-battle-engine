using CardWars.Core.Data;
using CardWars.Core.Storage;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class BattleInstanceProvider : ServerInstanceProvider<BattleInstance>
{
	public override BattleInstance Create(string id)
	{
		throw new NotImplementedException();
	}

	public override string? Save(BattleInstance serverInstance, StoragePath InstanceStoragePath)
	{
		throw new NotImplementedException();
	}
}
