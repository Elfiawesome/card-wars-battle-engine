using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class InstantiateBattlefieldHandler : ActionHandler<InstantiateBattlefieldData>
{
	public override bool Handle(GameState gameState, InstantiateBattlefieldData actionData)
	{
		if (gameState.Battlefields.ContainsKey(actionData.BattlefieldId)) { return false; }
		gameState.Battlefields[actionData.BattlefieldId] = new(gameState, actionData.BattlefieldId);
		return true;
	}
}

public record struct InstantiateBattlefieldData(
	BattlefieldId BattlefieldId
) : IActionData;
