using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public class SummonUnitResolver : Resolver
{
	public UnitSlotId TargetUnitSlotId;

	public SummonUnitResolver(UnitSlotId targetUnitSlotId)
	{
		TargetUnitSlotId = targetUnitSlotId;
	}

	public override void Resolve(GameState state)
	{
		AddActionBatch(new([
			
		]));
		CommitResolve();
	}
}