using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Rules;

public static class AttackRules
{
	public static bool CanTargetSlot(GameState state, EntityId targetSlotId)
	{
		if (state.Require<UnitSlot>(targetSlotId) is not { } targetSlot) return false;
		if (targetSlot.OwnerBattlefieldId is null) return false;

		var battlefield = state.Get<Battlefield>(targetSlot.OwnerBattlefieldId.Value);
		if (battlefield is null) return false;

		var targetRow = targetSlot.Position.Y;

		foreach (var slotId in battlefield.UnitSlotIds)
		{
			if (slotId == targetSlotId) continue;

			var slot = state.Get<UnitSlot>(slotId);
			if (slot is null || slot.Position.Y <= targetRow) continue;
			if (slot.HoldingCardId is null) continue;

			var card = state.Get<GenericCard>(slot.HoldingCardId.Value);
			if (card is not null && !card.IsFatal) return false;
		}

		return true;
	}
}
