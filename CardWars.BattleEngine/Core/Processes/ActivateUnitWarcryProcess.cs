using CardWars.BattleEngine.Core.GameState;

namespace CardWars.BattleEngine.Core.Processes;

public class ActivateUnitWarcryProcess : QueueGameProcess
{
	public AbilityStateId AbilityId;
	public ActivateUnitWarcryProcess(AbilityStateId abilityId)
	{
		AbilityId = abilityId;
	}

	public override void Execute(BattleEngine engine)
	{
		if (engine._gameState.Abilities.ContainsKey(AbilityId))
		{
			var ability = engine._gameState.Abilities[AbilityId];
			Console.WriteLine(ability);
		}
	}
}