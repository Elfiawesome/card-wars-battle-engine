using CardWars.BattleEngine.Core.Actions.ActionHandlers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public class SummonUnitResolver(UnitSlotId targetUnitSlotId, string unitDefinitionId_Todo) : Resolver
{
	public UnitSlotId TargetUniSlotId = targetUnitSlotId;
	string UnitDefinitionId_Todo = unitDefinitionId_Todo;

	public override void Resolve(GameState state)
	{
		var unitSlotId = TargetUniSlotId;
		if (state.UnitSlots.TryGetValue(unitSlotId, out var unitSlot))
		{
			var unitId = new UnitId(state.NewId);
			var abilityId = new AbilityId(state.NewId);
			// TOOD: This is where we will put the stuff from definition into the unit here
			AddActionBatch(new([
				new InstantiateUnitData(unitId, "Name here"),
				new AttachUnitToUnitSlotData(unitId, unitSlot.Id),
				new InstantiateAbilityData(abilityId, "custom"),
				new AttachAbilityToUnitData(abilityId, unitId)
			]));
			Commit();

			// Raise event to activate (MY OWN) and other abilities
			RaiseEventSignal(state.EventManager.OnUnitSummoned, new()
			{
				SummonedUnitId = unitId
			});

			CommitResolve();
		}
	}
}