using System.Security.Cryptography.X509Certificates;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AdvanceTurnOrderHandler : ActionHandler<AdvanceTurnOrderData>
{
	public override bool Handle(GameState gameState, AdvanceTurnOrderData actionData)
	{
		if (actionData.SetIndex != null)
		{
			gameState.TurnOrderIndex = actionData.SetIndex ?? 0;
		}
		else
		{
			gameState.TurnOrderIndex += 1;
			if (gameState.TurnOrderIndex >= gameState.TurnOrder.Count)
			{
				gameState.TurnOrderIndex = 0;
			}
		}

		gameState.AllowedPlayerInput.Clear();
		if (gameState.CurrentPlayer != null)
		{
			gameState.AllowedPlayerInput.Add((PlayerId)gameState.CurrentPlayer);
		}
		return true;
	}
}

public record struct AdvanceTurnOrderData(
	int? SetIndex = null
) : IActionData;