using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

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
		var fatalUnits = new List<(EntityId CardId, EntityId PlayerId, EntityId BattlefieldId, EntityId UnitSlotId)>();
		var discardBatch = new BlockBatch([]);

		foreach (var card in context.State.OfType<GenericCard>())
		{
			if (!card.IsUnit || !card.IsFatal || card.OwnerUnitSlotId is not { } unitSlotId) continue;
			if (context.State.Get<UnitSlot>(unitSlotId) is not { OwnerBattlefieldId: { } battlefieldId }) continue;
			if (context.State.Get<Battlefield>(battlefieldId) is not { OwnerPlayerId: { } playerId }) continue;

			discardBatch.Blocks.Add(new DetachCardFromUnitSlotBlock(unitSlotId, card.Id));
			discardBatch.Blocks.Add(new AttachCardToGraveyardBlock(playerId, card.Id));
			fatalUnits.Add((card.Id, playerId, battlefieldId, unitSlotId));
		}

		if (discardBatch.Blocks.Count > 0)
			context.ApplyBlockBatch(discardBatch);

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

		foreach (var card in context.State.OfType<GenericCard>())
		{
			if (!card.IsUnit || card.OwnerUnitSlotId is null) continue;

			context.QueueEvent(new ChargeRefillEvent()
			{
				CardId = card.Id,
				Amount = card.ChargeMax
			});
		}

		context.QueueEvent(new EndTurnEvent() { TurnState = request.TurnState, PhaseChanged = true });
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
