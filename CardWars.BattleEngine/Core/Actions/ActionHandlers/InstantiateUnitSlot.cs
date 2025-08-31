using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class InstantiateUnitSlotHandler : ActionHandler<InstantiateUnitSlotData>
{
	public override bool Handle(GameState gameState, InstantiateUnitSlotData actionData)
	{
		if (gameState.UnitSlots.ContainsKey(actionData.UnitSlotId)) { return false; }
		gameState.UnitSlots[actionData.UnitSlotId] = new(gameState, actionData.UnitSlotId);
		return true;
	}
}

public record struct InstantiateUnitSlotData(
	UnitSlotId UnitSlotId
) : IActionData;
