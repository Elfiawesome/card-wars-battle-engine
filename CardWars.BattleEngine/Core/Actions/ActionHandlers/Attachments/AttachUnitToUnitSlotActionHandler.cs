using CardWars.BattleEngine.Core.Actions.ActionDatas.Attachments;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers.Creations;

public class AttachUnitToUnitSlotActionHandler : ActionHandler<AttachUnitToUnitSlotActionData>
{
	public override void Handle(GameState gameState, EventManager eventManager, AttachUnitToUnitSlotActionData actionData)
	{
		Console.WriteLine($"[ACTION]: Attaching unit {actionData.UnitId} to unit slot {actionData.UnitSlotId}");

		if (gameState.UnitSlots.TryGetValue(actionData.UnitSlotId, out var unitSlot))
		{
			if (gameState.Units.TryGetValue(actionData.UnitId, out var unit))
			{
				unit.ParentUnitSlotId = actionData.UnitSlotId;
				unitSlot.HoldingUnit = actionData.UnitId;
			}
		}
	}
}