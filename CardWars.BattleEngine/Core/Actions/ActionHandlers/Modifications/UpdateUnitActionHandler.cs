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
	}
}