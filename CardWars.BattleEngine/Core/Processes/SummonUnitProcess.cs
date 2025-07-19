using CardWars.BattleEngine.Core.GameState;

namespace CardWars.BattleEngine.Core.Processes;

public class SummonUnitProcess : QueueGameProcess
{
	public override void Execute(BattleEngine engine)
	{
		// Need to make it as action not in code here
		UnitStateId unitId = new(Guid.NewGuid());
		engine.GameState.Units[unitId] = new UnitState(unitId);

		// Imagine we loaded the ability here
		AbilityStateId abilityId = new(Guid.NewGuid());
		engine.GameState.Abilities[abilityId] = new AbilityState(abilityId);

		// Hook both together
		engine.GameState.Abilities[abilityId].ParentUnitId = unitId;
		engine.GameState.Units[unitId].Abilities.Add(abilityId);

		engine.QueueProcess(
			new ActivateUnitWarcryProcess(abilityId)
		);

		Complete();
	}
}