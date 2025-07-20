using CardWars.BattleEngine.Core.Actions.ActionDatas;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public class SummonUnitResolver : Resolver
{
	public UnitSlotId TargetUnitSlotId;

	public SummonUnitResolver(UnitSlotId targetUnitSlotId)
	{
		TargetUnitSlotId = targetUnitSlotId;
	}

	public override void Resolve(GameState state, EventManager eventManager)
	{
		Console.WriteLine($"\n[     Let's summon a unit here at {TargetUnitSlotId}     ]\n");

		UnitId unitId = state.UnitIdCounter;
		AddActionBatch(new([
			new CreateUnitActionData(unitId),
			new AttachUnitToUnitSlotActionData(unitId, TargetUnitSlotId),
			new UpdateUnitActionData() {
				UnitId = unitId,
				Name = "Wowzies",
				Pt = 100,
			}
		]));
		CommitResolve();
	}
}