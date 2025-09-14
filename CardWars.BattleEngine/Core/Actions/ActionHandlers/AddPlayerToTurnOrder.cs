using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AddPlayerToTurnOrderHandler : ActionHandler<AddPlayerToTurnOrderData>
{
	public override bool Handle(GameState gameState, AddPlayerToTurnOrderData actionData)
	{
		gameState.TurnOrder.Add(actionData.PlayerId);
		return true;
	}
}

public record struct AddPlayerToTurnOrderData(
	PlayerId PlayerId
) : IActionData;