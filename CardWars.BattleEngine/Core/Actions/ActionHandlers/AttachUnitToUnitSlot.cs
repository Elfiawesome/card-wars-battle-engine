using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AttachUnitToUnitSlotHandler : ActionHandler<AttachUnitToUnitSlotData>
{
	public override bool Handle(GameState gameState, AttachUnitToUnitSlotData actionData)
	{
		if (gameState.UnitSlots.TryGetValue(actionData.UnitSlotId, out var unitSlot))
		{
			if (gameState.Units.TryGetValue(actionData.UnitId, out var unit))
			{
				unitSlot.HoldingUnit = unit.Id;
				return true;
			}
		}
		return false;
	}
}

public record struct AttachUnitToUnitSlotData(
	UnitId UnitId,
	UnitSlotId UnitSlotId
) : IActionData;
