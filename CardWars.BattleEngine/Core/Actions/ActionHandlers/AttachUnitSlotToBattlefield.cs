using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionHandlers;

public class AttachUnitSlotToBattlefieldHandler : ActionHandler<AttachUnitSlotToBattlefieldData>
{
	public override bool Handle(GameState gameState, AttachUnitSlotToBattlefieldData actionData)
	{
		if (gameState.Battlefields.TryGetValue(actionData.BattlefieldId, out var battlefield))
		{
			// Actually this check isn't needed but just in case
			if (gameState.UnitSlots.TryGetValue(actionData.UnitSlotId, out var unitSlot))
			{
				battlefield.UnitSlots.Add(actionData.UnitSlotId);
				return true;
			}
		}
		return false;
	}
}

public record struct AttachUnitSlotToBattlefieldData(
	UnitSlotId UnitSlotId,
	BattlefieldId BattlefieldId
) : IActionData;
