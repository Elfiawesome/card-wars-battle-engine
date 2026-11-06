using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class TurnEndedEvent : IEvent
{
	[DataTag] public EntityId PlayerId { get; set; }
	[DataTag] public TurnPhase Phase { get; set; }
	[DataTag] public int TurnNumber { get; set; }
}

[DataTagType()]
public class PhaseEndedEvent : IEvent
{
	[DataTag] public TurnPhase EndedPhase { get; set; }
	[DataTag] public TurnPhase NextPhase { get; set; }
	[DataTag] public required TurnState TurnState { get; set; }
}

[DataTagType()]
public class RoundEndedEvent : IEvent
{
	[DataTag] public required TurnState TurnState { get; set; }
}

[DataTagType()]
public class EndTurnEvent : IEvent
{
	[DataTag] public required TurnState TurnState { get; set; }
	[DataTag] public bool PhaseChanged { get; set; }
}

public class TurnEndedEventHandler : IEventHandler<TurnEndedEvent>
{
	public void Handle(Transaction context, TurnEndedEvent request)
	{
		var state = context.State;
		var endedPhase = state.Turn.Phase;
		var newTurnState = state.Turn.Clone();
		newTurnState.TurnIndex += 1;

		var phaseChanged = false;
		if (newTurnState.TurnIndex >= newTurnState.TurnOrder.Count)
		{
			newTurnState.TurnIndex = 0;
			newTurnState.TurnNumber += 1;
			newTurnState.Phase = newTurnState.Phase == TurnPhase.Setup
				? TurnPhase.Attacking
				: TurnPhase.Setup;
			phaseChanged = true;
		}

		if (endedPhase == TurnPhase.Attacking && newTurnState.Phase == TurnPhase.Setup)
			newTurnState.RoundNumber += 1;

		EntityId? currentPlayerId = newTurnState.TurnOrder.ElementAtOrDefault(newTurnState.TurnIndex);
		if (currentPlayerId is null || currentPlayerId == EntityId.None)
		{
			Log.Warn($"TurnOrder[{newTurnState.TurnIndex}] is null/empty, turn cannot advance");
			return;
		}

		newTurnState.AllowedPlayerInputs.Clear();
		newTurnState.AllowedPlayerInputs.Add((EntityId)currentPlayerId);

		if (phaseChanged)
		{
			context.QueueEvent(new PhaseEndedEvent()
			{
				EndedPhase = endedPhase,
				NextPhase = newTurnState.Phase,
				TurnState = newTurnState
			});

			if (endedPhase == TurnPhase.Attacking && newTurnState.Phase == TurnPhase.Setup)
			{
				context.QueueEvent(new RoundEndedEvent() { TurnState = newTurnState });
				return;
			}
		}

		context.QueueEvent(new EndTurnEvent() { TurnState = newTurnState, PhaseChanged = phaseChanged });
	}
}

public class PhaseEndedEventHandler : IEventHandler<PhaseEndedEvent>
{
	public void Handle(Transaction context, PhaseEndedEvent request)
	{
	}
}

public class RoundEndedEventHandler : IEventHandler<RoundEndedEvent>
{
	public void Handle(Transaction context, RoundEndedEvent request)
	{
		// Handle Fatal & Death units
		var fatalUnits = new List<(EntityId CardId, EntityId PlayerId, EntityId BattlefieldId, EntityId UnitSlotId)>();
		var fatalUnitsListTag = new ListTag();
		var discardBatch = new BlockBatch([]);

		foreach (var card in context.State.OfType<GenericCard>())
		{
			if (!card.IsUnit || !card.IsFatal || card.OwnerUnitSlotId is not { } unitSlotId) continue;
			if (context.State.Get<UnitSlot>(unitSlotId) is not { OwnerBattlefieldId: { } battlefieldId }) continue;
			if (context.State.Get<Battlefield>(battlefieldId) is not { OwnerPlayerId: { } playerId }) continue;

			discardBatch.Blocks.Add(new DetachCardFromUnitSlotBlock(unitSlotId, card.Id));
			discardBatch.Blocks.Add(new AttachCardToGraveyardBlock(playerId, card.Id));
			fatalUnits.Add((card.Id, playerId, battlefieldId, unitSlotId));
			fatalUnitsListTag.Add(card.Id.Value);
		}

		if (fatalUnits.Count > 0)
		{
			discardBatch.AnimationId = SharedIds.Animations.Death.ToString();
			discardBatch.AnimationArgs.Set("units", fatalUnitsListTag);
			context.ApplyBlockBatch(discardBatch);
		}

		foreach (var fatalUnit in fatalUnits)
		{
			context.QueueEvent(new UnitDiedEvent()
			{
				CardId = fatalUnit.CardId,
				PlayerId = fatalUnit.PlayerId,
				BattlefieldId = fatalUnit.BattlefieldId,
				UnitSlotId = fatalUnit.UnitSlotId
			});
		}

		// Handle hero defeat and player loss
		var heroEvaluator = new HeroLoseConditionEvaluator(context.Registry.DataDriven);
		var defeatedHeroes = new List<(EntityId HeroCardId, EntityId PlayerId, EntityId BattlefieldId, int Hrt)>();
		var defeatedPlayers = new List<(EntityId PlayerId, int Team)>();
		var defeatBatch = new BlockBatch([]);

		foreach (var player in context.State.OfType<Player>())
		{
			if (player.IsDefeated) continue;

			var heroes = GetPlayerHeroes(context.State, player);
			var defeatedHeroIds = new HashSet<EntityId>(player.DefeatedHeroIds);

			foreach (var (hero, battlefieldId) in heroes)
			{
				if (defeatedHeroIds.Contains(hero.Id)) continue;
				if (!heroEvaluator.IsDefeated(context.State, hero)) continue;

				defeatedHeroIds.Add(hero.Id);
				defeatBatch.Blocks.Add(new MarkHeroDefeatedBlock(player.Id, hero.Id));
				defeatedHeroes.Add((hero.Id, player.Id, battlefieldId, hero.Hrt));
			}

			if (heroes.Any(hero => !defeatedHeroIds.Contains(hero.Hero.Id))) continue;

			defeatedPlayers.Add((player.Id, player.Team));
			defeatBatch.Blocks.Add(new MarkPlayerDefeatedBlock(player.Id));
		}

		if (defeatBatch.Blocks.Count > 0) context.ApplyBlockBatch(defeatBatch);

		foreach (var defeatedHero in defeatedHeroes)
		{
			context.QueueEvent(new HeroDiedEvent()
			{
				HeroCardId = defeatedHero.HeroCardId,
				PlayerId = defeatedHero.PlayerId,
				BattlefieldId = defeatedHero.BattlefieldId,
				Hrt = defeatedHero.Hrt
			});
		}

		foreach (var defeatedPlayer in defeatedPlayers)
		{
			context.QueueEvent(new PlayerDefeatedEvent()
			{
				PlayerId = defeatedPlayer.PlayerId,
				Team = defeatedPlayer.Team
			});
		}

		// Win Check
		context.QueueEvent(new WinConditionCheckEvent() { TurnState = request.TurnState });
	}

	private static List<(GenericCard Hero, EntityId BattlefieldId)> GetPlayerHeroes(GameState state, Player player)
	{
		var heroes = new List<(GenericCard Hero, EntityId BattlefieldId)>();

		foreach (var battlefieldId in player.BattlefieldIds)
		{
			if (state.Get<Battlefield>(battlefieldId) is not { } battlefield) continue;

			foreach (var heroId in battlefield.HeroCardIds)
			{
				if (state.Get<GenericCard>(heroId) is { IsHero: true } hero)
					heroes.Add((hero, battlefield.Id));
			}
		}

		return heroes;
	}

}

public class EndTurnEventHandler : IEventHandler<EndTurnEvent>
{
	public void Handle(Transaction context, EndTurnEvent request)
	{
		context.ApplyBlockBatch(new BlockBatch([
			new UpdateTurnStateBlock(request.TurnState)
		]));
	}
}
