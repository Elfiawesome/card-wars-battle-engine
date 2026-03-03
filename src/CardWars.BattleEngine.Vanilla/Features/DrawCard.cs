using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Features;

// --- Request to draw the card (so that behaviours can intercept and reject the card request) ---
public record struct DrawCardRequestInput(
	EntityId DeckId,
	EntityId ReceivedPlayerId // Since we might draw a card as someone else for another deck
) : IInput;

public class DrawCardRequestInputHandler : IInputHandler<DrawCardRequestInput>
{
	public void Handle(InputContext context, DrawCardRequestInput request)
	{
		var deck = context.Transaction.State.Get<Deck>(request.DeckId);
		if (deck == null) return;
		context.Transaction.QueueEvent(new DrawCardRequestEvent() { PlayerId = request.ReceivedPlayerId, DeckId = request.DeckId });
	}
}

public class DrawCardRequestEvent() : IEvent
{
	public required EntityId PlayerId;
	public required EntityId DeckId;
	public bool IsCancelled = true;
	public double LuckRate = 0.0; // <- used as a way to determin luck later on
}

public class DrawCardRequestEventHandler : IEventHandler<DrawCardRequestEvent>
{
	public void Handle(Transaction context, DrawCardRequestEvent request)
	{
		var deck = context.State.Get<Deck>(request.DeckId);
		if (deck == null) return;
		
		Console.WriteLine($"A player drew a card from deck {request.DeckId}");
		var batch = new BlockBatch([]);
		
		var cardId = deck.CardIds.FirstOrDefault(Guid.Empty);
		if (cardId == Guid.Empty) return;
		
		batch.Blocks.Add(new DetachCardFromDeckBlock(request.DeckId, cardId));
		batch.Blocks.Add(new AttachCardToPlayerBlock(request.PlayerId, cardId));
		context.ApplyBlockBatch(batch);
		
		context.QueueEvent(new DrawCardEvent());
	}
}

// --- Post event ---

public class DrawCardEvent() : IEvent
{
}
