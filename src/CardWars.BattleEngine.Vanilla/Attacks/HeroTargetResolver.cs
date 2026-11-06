using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Rules;

namespace CardWars.BattleEngine.Vanilla.Attacks;

public static class HeroTargetResolver
{
	public static bool CanTarget(GameState state, GenericCard attacker, EntityId heroCardId, DataDrivenRegistry dataDriven)
	{
		if (AttackTargetResolver.GetOwnerPlayerId(state, attacker) is not { } attackerPlayerId) return false;
		if (state.Get<Player>(attackerPlayerId) is not { } attackerPlayer) return false;

		if (state.Get<GenericCard>(heroCardId) is not { IsHero: true } hero) return false;
		if (new HeroLoseConditionEvaluator(dataDriven).IsDefeated(state, hero)) return false;
		if (hero.OwnerBattlefieldId is not { } battlefieldId) return false;
		if (state.Get<Battlefield>(battlefieldId) is not { } battlefield) return false;
		if (battlefield.OwnerPlayerId is not { } targetPlayerId) return false;
		if (state.Get<Player>(targetPlayerId) is not { } targetPlayer) return false;

		if (attackerPlayer.Team == targetPlayer.Team) return false;

		foreach (var slotId in battlefield.UnitSlotIds)
		{
			if (state.Get<UnitSlot>(slotId) is not { } slot) continue;
			if (slot.HoldingCardId is not { } cardId) continue;

			var card = state.Get<GenericCard>(cardId);
			if (card is not null && !card.IsFatal) return false;
		}

		return true;
	}

	public static List<EntityId> GetValidTargets(GameState state, GenericCard attacker, DataDrivenRegistry dataDriven)
	{
		var targets = new List<EntityId>();

		foreach (var battlefield in state.OfType<Battlefield>())
		{
			foreach (var heroId in battlefield.HeroCardIds)
			{
				if (CanTarget(state, attacker, heroId, dataDriven)) targets.Add(heroId);
			}
		}

		return targets;
	}
}
