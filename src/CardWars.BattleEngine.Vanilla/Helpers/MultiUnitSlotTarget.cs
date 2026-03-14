using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Helpers;

public static class MultiUnitSlotTargetSystem
{
	public static Func<Battlefield, UnitSlot, UnitSlot, bool> Algo(string targetType) => targetType switch
	{
		"row"
			=> (_, otherUnitSlot, targetUnitSlot) => otherUnitSlot.Position.Y == targetUnitSlot.Position.Y,
		"column"
			=> (_, otherUnitSlot, targetUnitSlot) => otherUnitSlot.Position.X == targetUnitSlot.Position.X,
		"all"
			=> (_, otherUnitSlot, targetUnitSlot) => true,
		_ => (_, _, _) => false,
	};

	public static List<EntityId> ListUnitSlots(GameState state, EntityId unitSlotId, string targetType)
	{
		// Get Unit Slot
		var unitSlot = state.Get<UnitSlot>(unitSlotId);
		if (unitSlot == null) { return []; }

		// Get parent Battlefield
		if (unitSlot.OwnerBattlefieldId == null) { return []; }
		var battlefield = state.Get<Battlefield>((EntityId)unitSlot.OwnerBattlefieldId);
		if (battlefield == null) { return []; }

		// Get all victims
		HashSet<EntityId> targettedIds = [];

		foreach (var usid in battlefield.UnitSlotIds)
		{
			// Check if Unit Slot is valid
			var us = state.Get<UnitSlot>(usid);
			if (us == null) { continue; }

			// If X is the same (aka same column)
			var isUnitSlotTargettedToo = Algo(targetType).Invoke(battlefield, us, unitSlot);
			if (isUnitSlotTargettedToo)
			{
				targettedIds.Add(us.Id);
			}
		}

		return [.. targettedIds];
	}
}