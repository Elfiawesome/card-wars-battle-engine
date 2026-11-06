using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public static class AttackDamageResolver
{
	public static int GetOriginDamage(GenericCard attacker, AttackProfile attack)
	{
		var damage = attacker.Atk;

		foreach (var pattern in attack.Patterns)
		{
			var hitsOrigin = pattern.Type switch
			{
				AttackPatternType.Single => true,
				AttackPatternType.Row => true,
				AttackPatternType.Column => true,
				AttackPatternType.All => true,
				AttackPatternType.Cell => pattern.X == 0 && pattern.Y == 0,
				_ => false
			};

			if (hitsOrigin)
				damage += pattern.Damage.Resolve(attacker.Atk);
		}

		return damage;
	}

	public static Dictionary<EntityId, int> Resolve(
		GameState state,
		GenericCard attacker,
		AttackProfile attack,
		EntityId targetSlotId)
	{
		var result = new Dictionary<EntityId, int>();
		if (state.Get<UnitSlot>(targetSlotId) is not { } origin) return result;

		Add(result, origin.Id, attacker.Atk);

		foreach (var pattern in attack.Patterns)
		{
			var damage = pattern.Damage.Resolve(attacker.Atk);
			if (damage <= 0) continue;

			foreach (var slot in ResolvePattern(state, origin, pattern))
				Add(result, slot.Id, damage);
		}

		return result;
	}

	private static IEnumerable<UnitSlot> ResolvePattern(
		GameState state,
		UnitSlot origin,
		AttackPattern pattern)
	{
		if (pattern.Type == AttackPatternType.Single)
		{
			yield return origin;
			yield break;
		}

		if (origin.OwnerBattlefieldId is not { } battlefieldId) yield break;
		if (state.Get<Battlefield>(battlefieldId) is not { } battlefield) yield break;

		foreach (var slotId in battlefield.UnitSlotIds)
		{
			if (state.Get<UnitSlot>(slotId) is not { } slot) continue;

			var matches = pattern.Type switch
			{
				AttackPatternType.Row => slot.Position.Y == origin.Position.Y,
				AttackPatternType.Column => slot.Position.X == origin.Position.X,
				AttackPatternType.All => true,
				AttackPatternType.Cell => slot.Position.X == origin.Position.X + pattern.X
					&& slot.Position.Y == origin.Position.Y + pattern.Y,
				_ => false
			};

			if (matches) yield return slot;
		}
	}

	private static void Add(Dictionary<EntityId, int> result, EntityId slotId, int damage)
	{
		if (damage <= 0) return;
		result[slotId] = result.GetValueOrDefault(slotId) + damage;
	}
}
