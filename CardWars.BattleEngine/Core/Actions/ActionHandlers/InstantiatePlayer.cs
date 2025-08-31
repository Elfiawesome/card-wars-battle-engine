using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class InstantiatePlayerHandler : ActionHandler<InstantiatePlayerData>
{
	public override bool Handle(GameState gameState, InstantiatePlayerData actionData)
	{
		if (gameState.Players.ContainsKey(actionData.PlayerId)) { return false; }
		gameState.Players[actionData.PlayerId] = new(gameState, actionData.PlayerId);
		return true;
	}
}

public record struct InstantiatePlayerData(
	PlayerId PlayerId
) : IActionData;
