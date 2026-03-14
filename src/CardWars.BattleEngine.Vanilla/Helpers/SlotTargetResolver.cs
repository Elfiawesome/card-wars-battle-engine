using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Helpers;

/// <summary>
/// Given a target slot and an attack pattern name (e.g. "single", "row", "column", "all"),
/// resolves the full list of affected slot EntityIds.
/// </summary>
public static class SlotTargetResolver
{
	public static Func<Battlefield, UnitSlot, UnitSlot, bool> PatternPredicate(string pattern)
		=> pattern switch
		{
			"row" => (_, origin, candidate) => candidate.Position.Y == origin.Position.Y && candidate.OwnerBattlefieldId == origin.OwnerBattlefieldId,
			"column" => (_, origin, candidate) => candidate.Position.X == origin.Position.X && candidate.OwnerBattlefieldId == origin.OwnerBattlefieldId,
			"all" => (_, _, _) => true,
			_ => (_, origin, candidate) => candidate.Id == origin.Id, // "single" / default
		};

	public static List<EntityId> Resolve(GameState state, EntityId originSlotId, string pattern)
	{
		if (state.Require<UnitSlot>(originSlotId) is not { } originSlot)
			return [];

		var predicate = PatternPredicate(pattern);
		var result = new List<EntityId>();

		foreach (var battlefield in state.OfType<Battlefield>())
		{
			foreach (var slotId in battlefield.UnitSlotIds)
			{
				if (state.Require<UnitSlot>(slotId) is not { } slot) continue;
				if (predicate(battlefield, originSlot, slot))
					result.Add(slot.Id);
			}
		}

		return result;
	}
}