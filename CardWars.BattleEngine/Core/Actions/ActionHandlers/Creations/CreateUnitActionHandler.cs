using CardWars.BattleEngine.Core.Actions.ActionDatas.Creations;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers.Creations;

public class CreateUnitActionHandler : ActionHandler<CreateUnitActionData>
{
	public override void Handle(GameState gameState, EventManager eventManager, CreateUnitActionData actionData)
	{
		Console.WriteLine($"[ACTION]: Creating UNIT ({actionData.UnitId})");
	}
}