using CardWars.BattleEngine.Core.Actions.ActionDatas.Modifications;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers.Modifications;

public class UpdateUnitActionHandler : ActionHandler<UpdateUnitActionData>
{
	public override void Handle(GameState gameState, EventManager eventManager, UpdateUnitActionData actionData)
	{
		string changes = $"ID: {actionData.UnitId}, " +
			$"Name: {actionData.Name}, " +
			$"Hp: {actionData.Hp}, " +
			$"Atk: {actionData.Atk}, " +
			$"Pt: {actionData.Pt}";

		Console.WriteLine($"[ACTION]: Updating UNIT with ({changes})");

		if (gameState.Units.TryGetValue(actionData.UnitId, out var unit))
		{
			if (actionData.Name != null) unit.Name = actionData.Name;
			if (actionData.Hp != null) unit.Hp = actionData.Hp ?? 0;
			if (actionData.Atk != null) unit.Atk = actionData.Atk ?? 0;
			if (actionData.Pt != null) unit.Pt = actionData.Pt ?? 0;
		}
	}
}