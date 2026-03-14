using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Features;

// --- Request to draw the card (so that behaviours can intercept and reject the card request) ---
[DataTagType()]
public record struct DrawCardRequestInput(
	[property: DataTag] EntityId DeckId,
	[property: DataTag] EntityId ReceivedPlayerId // Since we might draw a card as someone else for another deck
) : IInput;

public class DrawCardRequestInputHandler : IInputHandler<DrawCardRequestInput>
{
	public void Handle(InputContext context, DrawCardRequestInput request)
	{
		if (context.Transaction.State.Require<Deck>(request.DeckId) is not { } deck) return;
		context.Transaction.QueueEvent(new DrawCardRequestEvent() { PlayerId = request.ReceivedPlayerId, DeckId = request.DeckId });
	}
}

[DataTagType()]
public class DrawCardRequestEvent() : IEvent
{
	[DataTag] public required EntityId PlayerId { get; set; }
	[DataTag] public required EntityId DeckId { get; set; }
	[DataTag] public bool IsCancelled { get; set; } = false;
	[DataTag] public double LuckRate { get; set; } = 0.0; // <- used as a way to determin luck later on
}

public class DrawCardRequestEventHandler : IEventHandler<DrawCardRequestEvent>
{
	public void Handle(Transaction context, DrawCardRequestEvent request)
	{
		if (context.State.Require<Player>(request.PlayerId) is not { } player) return;
		if (context.State.Require<Deck>(request.DeckId) is not { } deck) return;

		var batch = new BlockBatch([]);

		var cardId = deck.CardIds.FirstOrDefault(EntityId.None);
		if (cardId == EntityId.None) { Logger.Warn($"Deck [{request.DeckId}] is empty, cannot draw"); return; }

		batch.Blocks.Add(new DetachCardFromDeckBlock(request.DeckId, cardId));
		batch.Blocks.Add(new AttachCardToPlayerBlock(request.PlayerId, cardId));
		context.ApplyBlockBatch(batch);

		context.QueueEvent(new DrawCardEvent());
	}
}