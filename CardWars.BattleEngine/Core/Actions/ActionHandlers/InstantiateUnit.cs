using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class InstantiateUnitHandler : ActionHandler<InstantiateUnitData>
{
	public override bool Handle(GameState gameState, InstantiateUnitData actionData)
	{
		if (gameState.Units.ContainsKey(actionData.UnitId)) { return false; }
		gameState.Units[actionData.UnitId] = new(gameState, actionData.UnitId);
		return true;
	}
}

public record struct InstantiateUnitData(
	UnitId UnitId,
	string UnitName
) : IActionData;
