using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Targeting;

namespace CardWars.BattleEngine.Vanilla.Rules;

public sealed class BattleActionValidator(BattleEngineRegistry registry)
{
	private readonly AttackResolver _attackResolver = new(registry);
	private readonly CardTargetFilterEvaluator _targetFilter = new(registry.DataDriven);

	public bool CanAct(GameState state, EntityId playerId)
		=> state.Turn.AllowedPlayerInputs.Contains(playerId);

	public bool CanStartPlayCard(
		GameState state,
		EntityId playerId,
		GenericCard card,
		out string? reason)
	{
		reason = null;

		if (!CanAct(state, playerId))
		{
			reason = $"player [{playerId}] is not allowed to act";
			return false;
		}

		if (card.OwnerPlayerId != playerId)
		{
			reason = $"card [{card.Id}] is not owned by player [{playerId}]";
			return false;
		}

		if (state.Get<Player>(playerId) is not { } player)
		{
			reason = $"player [{playerId}] was not found";
			return false;
		}

		if (!player.HandCardIds.Contains(card.Id))
		{
			reason = $"card [{card.Id}] is not in player [{playerId}]'s hand";
			return false;
		}

		if (card.Pt > player.Pt)
		{
			reason = $"card [{card.Id}] costs {card.Pt} Pt but player [{playerId}] has {player.Pt}";
			return false;
		}

		return true;
	}

	public bool CanPlayCard(
		GameState state,
		EntityId playerId,
		GenericCard card,
		EntityId? targetId,
		out string? reason)
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

	public bool TryResolveAttack(
		GameState state,
		EntityId playerId,
		GenericCard attacker,
		out AttackProfile attack,
		out string? reason)
	{
		reason = null;
		attack = _attackResolver.Resolve(attacker);

		if (!CanAct(state, playerId))
		{
			reason = $"player [{playerId}] is not allowed to act";
			return false;
		}

		if (state.Turn.Phase != TurnPhase.Attacking)
		{
			reason = "the current phase is not Attacking";
			return false;
		}

		if (AttackTargetResolver.GetOwnerPlayerId(state, attacker) != playerId)
		{
			reason = $"attacker [{attacker.Id}] is not owned by player [{playerId}]";
			return false;
		}

		if (!_attackResolver.CanAfford(attacker, attack))
		{
			reason = $"attacker [{attacker.Id}] cannot afford [{attack.Name}]";
			return false;
		}

		return true;
	}

	public bool CanTargetUnit(
		GameState state,
		GenericCard attacker,
		AttackProfile attack,
		EntityId targetSlotId,
		out string? reason)
	{
		if (AttackTargetResolver.CanTarget(state, attacker, attack, targetSlotId))
		{
			reason = null;
			return true;
		}

		reason = $"target [{targetSlotId}] is not valid for card [{attacker.Id}]";
		return false;
	}

	public IReadOnlyCollection<EntityId> GetValidUnitTargets(GameState state, GenericCard attacker, AttackProfile attack)
		=> AttackTargetResolver.GetValidTargets(state, attacker, attack);

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
}
