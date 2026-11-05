using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Helpers;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public static class AttackTargetResolver
{
	public static EntityId? GetOwnerPlayerId(GameState state, GenericCard card)
	{
		if (card.OwnerUnitSlotId is { } slotId && state.Get<UnitSlot>(slotId) is { } slot)
			return GetOwnerPlayerId(state, slot);

		return card.OwnerPlayerId;
	}

	public static EntityId? GetOwnerPlayerId(GameState state, UnitSlot slot)
	{
		if (slot.OwnerBattlefieldId is not { } battlefieldId) return null;
		return state.Get<Battlefield>(battlefieldId)?.OwnerPlayerId;
	}

	public static bool CanTarget(GameState state, GenericCard attacker, AttackProfile attack, EntityId targetSlotId)
	{
		if (GetOwnerPlayerId(state, attacker) is not { } attackerPlayerId) return false;
		if (state.Get<Player>(attackerPlayerId) is not { } attackerPlayer) return false;

		if (state.Get<UnitSlot>(targetSlotId) is not { } targetSlot) return false;
		if (targetSlot.HoldingCardId is not { } targetCardId) return false;
		if (state.Get<GenericCard>(targetCardId) is null) return false;
		if (targetSlot.OwnerBattlefieldId is not { } targetBattlefieldId) return false;
		if (state.Get<Battlefield>(targetBattlefieldId) is not { } targetBattlefield) return false;
		if (targetBattlefield.OwnerPlayerId is not { } targetPlayerId) return false;
		if (state.Get<Player>(targetPlayerId) is not { } targetPlayer) return false;

		if (attackerPlayer.Team == targetPlayer.Team) return false;
		if (!attack.Bypass && !AttackRules.CanTargetSlot(state, targetSlotId)) return false;

		return true;
	}

	public static List<EntityId> GetValidTargets(GameState state, GenericCard attacker, AttackProfile attack)
		=> [.. state.OfType<UnitSlot>()
			.Where(slot => CanTarget(state, attacker, attack, slot.Id))
			.Select(slot => slot.Id)];
}
