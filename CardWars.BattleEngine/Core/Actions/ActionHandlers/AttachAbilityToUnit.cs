using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AttachAbilityToUnitHandler : ActionHandler<AttachAbilityToUnitData>
{
	public override bool Handle(GameState gameState, AttachAbilityToUnitData actionData)
	{
		if (gameState.Units.TryGetValue(actionData.UnitId, out var unit))
		{
			if (gameState.Abilities.TryGetValue(actionData.AbilityId, out var ability))
			{
				unit.Abilities.Add(ability.Id);
				return true;
			}
		}
		return false;
	}
}

public record struct AttachAbilityToUnitData(
	AbilityId AbilityId,
	UnitId UnitId
) : IActionData;
