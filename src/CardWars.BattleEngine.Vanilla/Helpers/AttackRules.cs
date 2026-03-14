using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Helpers;

public static class AttackRules
{
	/// <summary>
	/// A unit slot is targetable only if ALL rows with a higher Y (closer to front)
	/// have no non-FATAL units.
	/// </summary>
	public static bool CanTargetSlot(GameState state, EntityId targetSlotId)
	{
		if (state.Require<UnitSlot>(targetSlotId) is not { } targetSlot) return false;
		if (targetSlot.OwnerBattlefieldId == null) return false;

		var battlefield = state.Get<Battlefield>(targetSlot.OwnerBattlefieldId.Value);
		if (battlefield == null) return false;

		int targetRow = targetSlot.Position.Y;

		foreach (var slotId in battlefield.UnitSlotIds)
		{
			if (slotId == targetSlotId) continue;

			var slot = state.Get<UnitSlot>(slotId);
			if (slot == null || slot.Position.Y <= targetRow) continue; // same or behind

			if (slot.HoldingCardId == null) continue;

			var card = state.Get<GenericCard>(slot.HoldingCardId.Value);
			if (card != null && !card.IsFatal)
				return false; // blocked by a living unit in a front row
		}

		return true;
	}

	/// <summary>
	/// A hero can only be attacked if every unit slot on its battlefield
	/// is empty or holds a FATAL unit.
	/// </summary>
	public static bool CanTargetHero(GameState state, EntityId heroCardId)
	{
		foreach (var bf in state.OfType<Battlefield>())
		{
			if (!bf.HeroCardIds.Contains(heroCardId)) continue;

			foreach (var slotId in bf.UnitSlotIds)
			{
				var slot = state.Get<UnitSlot>(slotId);
				if (slot?.HoldingCardId == null) continue;

				var card = state.Get<GenericCard>(slot.HoldingCardId.Value);
				if (card != null && !card.IsFatal)
					return false;
			}

			return true; // all slots clear
		}

		return false; // hero not on any battlefield
	}

	/// <summary>
	/// Returns every valid BA target on opposing battlefields.
	/// Walks rows front-to-back; the first row with a living unit blocks everything behind it.
	/// If all rows are clear, heroes become targetable.
	/// </summary>
	public static List<EntityId> GetValidTargets(GameState state, EntityId attackerPlayerId)
	{
		var targets = new List<EntityId>();

		foreach (var bf in state.OfType<Battlefield>())
		{
			if (bf.OwnerPlayerId == attackerPlayerId) continue; // skip own battlefield

			// bucket slots by row
			var rows = new SortedDictionary<int, List<(UnitSlot slot, GenericCard? card)>>(
				Comparer<int>.Create((a, b) => b.CompareTo(a))); // descending = front first

			foreach (var slotId in bf.UnitSlotIds)
			{
				if (state.Require<UnitSlot>(slotId) is not { } slot) continue;

				GenericCard? card = slot.HoldingCardId != null
					? state.Get<GenericCard>(slot.HoldingCardId.Value)
					: null;

				if (!rows.ContainsKey(slot.Position.Y))
					rows[slot.Position.Y] = [];
				rows[slot.Position.Y].Add((slot, card));
			}

			bool blocked = false;
			foreach (var (_, rowSlots) in rows)
			{
				foreach (var (_, card) in rowSlots)
				{
					if (card != null && !card.IsFatal)
					{
						targets.Add(card.Id);
						blocked = true;
					}
				}
				if (blocked) break; // front row has living units → stop
			}

			if (!blocked)
			{
				// all rows clear → heroes are targetable
				foreach (var heroId in bf.HeroCardIds)
				{
					var hero = state.Get<GenericCard>(heroId);
					if (hero != null) targets.Add(heroId);
				}
			}
		}

		return targets;
	}
}