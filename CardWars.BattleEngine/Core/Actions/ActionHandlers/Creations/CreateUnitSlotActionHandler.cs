using CardWars.BattleEngine.Core.Actions.ActionDatas.Creations;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers.Creations;

public class CreateUnitSlotActionHandler : ActionHandler<CreateUnitSlotActionData>
{
	public override void Handle(GameState gameState, EventManager eventManager, CreateUnitSlotActionData actionData)
	{
		Console.WriteLine($"[ACTION]: Creating UNITSLOT ({actionData.UnitSlotId})");
	}
}