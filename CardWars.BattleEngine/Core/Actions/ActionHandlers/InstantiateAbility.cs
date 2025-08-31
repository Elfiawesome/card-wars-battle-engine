using CardWars.BattleEngine.Core.Custom;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class InstantiateAbilityHandler : ActionHandler<InstantiateAbilityData>
{
	public override bool Handle(GameState gameState, InstantiateAbilityData actionData)
	{
		if (gameState.Abilities.ContainsKey(actionData.AbilityId)) { return false; }
		if (actionData.DebugCustomType == "")
		{
			gameState.Abilities[actionData.AbilityId] = new(gameState, actionData.AbilityId);
		}
		else
		{
			gameState.Abilities[actionData.AbilityId] = new CustomAbility(gameState, actionData.AbilityId);
		}
		return true;
	}
}

public record struct InstantiateAbilityData(
	AbilityId AbilityId,
	string DebugCustomType = ""
) : IActionData;
