using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Targeting;

namespace CardWars.BattleEngine.Vanilla.Rules;

public sealed class BattleActionValidator(BattleEngineRegistry registry)
{
	private readonly AttackResolver _attackResolver = new(registry);
	private readonly CardTargetFilterEvaluator _targetFilter = new(registry.DataDriven);

	// --- Playing Cards ---
	// If a player can do inputs
	public bool CanAct(GameState state, EntityId playerId)
		=> state.Turn.AllowedPlayerInputs.Contains(playerId);

	public bool CanAct(GameState state, EntityId playerId, TurnPhase phase)
			=> CanAct(state, playerId) && phase == state.Turn.Phase;

	// If a player can play THIS card
	public bool CanStartPlayCard(GameState state, EntityId playerId, GenericCard card, out string? reason)
	{
		reason = null;
		// 1) Not allowed in inputs
		if (!CanAct(state, playerId)) { reason = $"player [{playerId}] is not allowed to act"; return false; }
		// 2) Doesnt even exist
		if (state.Get<Player>(playerId) is not { } player) { reason = $"player [{playerId}] was not found"; return false; }
		// 3) Card deosnt even belong to this player (Card -> Player relationship check)
		if (card.OwnerPlayerId != playerId) { reason = $"card [{card.Id}] is not owned by player [{playerId}]"; return false; }
		// 4) Card deosnt even belong in the hands of the player (Player -> Card relationship check)
		if (!player.HandCardIds.Contains(card.Id)) { reason = $"card [{card.Id}] is not in player [{playerId}]'s hand"; return false; }
		// 5) Not enough points.... Maybe to change to data drive?
		if (card.Pt > player.Pt) { reason = $"card [{card.Id}] costs {card.Pt} Pt but player [{playerId}] has {player.Pt}"; return false; }

		return true;
	}

	// Can play this card + the card's target individual evaluator
	public bool CanPlayCard(GameState state, EntityId playerId, GenericCard card, EntityId? targetId, out string? reason)
	{
		if (!CanStartPlayCard(state, playerId, card, out reason)) return false;

		if (!_targetFilter.CanTarget(state, card, playerId, targetId))
		{
			reason = $"target [{targetId}] is not valid for card [{card.Id}]";
			return false;
		}

		return true;
	}

	public IReadOnlyCollection<EntityId> GetPlayableTargets(GameState state, EntityId playerId, GenericCard card)
		=> _targetFilter.ResolveTargets(state, card, playerId);


	// --- Attacking Cards ---
	public bool TryResolveAttack(GameState state, EntityId playerId, GenericCard attacker, out AttackProfile attack, out string? reason)
	{
		reason = null;
		attack = _attackResolver.Resolve(attacker);
		// 1) Not allowed in inputs
		if (!CanAct(state, playerId)) { reason = $"player [{playerId}] is not allowed to act"; return false; }
		// 2) Only allowed in attacking phase
		if (state.Turn.Phase != TurnPhase.Attacking) { reason = "the current phase is not Attacking"; return false; }
		// 3) Attacher not owned by player
		if (state.GetParent<Player>(attacker)?.Id != playerId) { reason = $"attacker [{attacker.Id}] is not owned by player [{playerId}]"; return false; }
		// 4) Not enough pt
		if (!_attackResolver.CanAfford(attacker, attack)) { reason = $"attacker [{attacker.Id}] cannot afford [{attack.Definition.Name}]"; return false; }

		return true;
	}

	public bool CanTargetUnit(GameState state, GenericCard attacker, AttackProfile attack, EntityId targetSlotId, out string? reason)
	{
		if (CanTarget(state, attacker, attack, targetSlotId))
		{
			reason = null;
			return true;
		}

		reason = $"target [{targetSlotId}] is not valid for card [{attacker.Id}]";
		return false;
	}

	public IReadOnlyCollection<EntityId> GetValidUnitTargets(GameState state, GenericCard attacker, AttackProfile attack)
		=> [.. state.OfType<UnitSlot>()
			.Where(slot => CanTarget(state, attacker, attack, slot.Id))
			.Select(slot => slot.Id)];

	public IReadOnlyCollection<EntityId> GetValidHeroTargets(GameState state, GenericCard attacker)
		=> HeroTargetResolver.GetValidTargets(state, attacker, registry);

	public bool CanTargetHero(
		GameState state,
		GenericCard attacker,
		GenericCard hero,
		out string? reason)
	{
		if (HeroTargetResolver.CanTarget(state, attacker, hero.Id, registry))
		{
			reason = null;
			return true;
		}

		reason = $"hero [{hero.Id}] is not a valid target for card [{attacker.Id}]";
		return false;
	}

	public static bool CanTarget(GameState state, GenericCard attacker, AttackProfile attack, EntityId targetSlotId)
	{
		if (state.GetParent<Player>(attacker) is not { } attackerPlayer) return false;

		if (state.Get<UnitSlot>(targetSlotId) is not { } targetSlot) return false;
		if (targetSlot.HoldingCardId is not { } targetCardId) return false;
		if (state.Get<GenericCard>(targetCardId) is null) return false;
		if (state.GetParent<Player>(targetSlot) is not { } targetPlayer) return false;

		if (attackerPlayer.Team == targetPlayer.Team) return false;
		if (!attack.Definition.Bypass && !CanTargetSlot(state, targetSlotId)) return false;

		return true;
	}

	public static bool CanTargetSlot(GameState state, EntityId targetSlotId)
	{
		if (state.Require<UnitSlot>(targetSlotId) is not { } targetSlot) return false;
		if (state.GetParent<Battlefield>(targetSlot) is not { } battlefield) return false;

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
