using CardWars.BattleEngine.Core.Actions.ActionDatas.Modifications;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers.Modifications;

public class UpdateUnitActionHandler : ActionHandler<UpdateUnitActionData>
{
	public override void Handle(GameState gameState, EventManager eventManager, UpdateUnitActionData actionData)
	{
		Console.WriteLine($"[ACTION]: Updating UNIT Name -> ({actionData.Name})");
	}
}