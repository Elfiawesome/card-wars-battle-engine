using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public class BattleInstanceProvider : ServerInstanceProvider<BattleInstance, Guid>
{
	protected override BattleInstance Create(Guid battleId)
		=> new BattleInstance { InstanceId = battleId };

	protected override void Save(BattleInstance instance) { }
}
