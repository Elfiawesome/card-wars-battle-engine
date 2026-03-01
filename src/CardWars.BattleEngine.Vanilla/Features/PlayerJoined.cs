using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;

namespace CardWars.BattleEngine.Vanilla.Features;

public class PlayerJoinedEvent() : IEvent
{
	public EntityId PlayerId;
};

public class PlayerJoinedEventHandler : IEventHandler<PlayerJoinedEvent>
{
	// TODO: Remove starter deck with player's deck itself. Using this as a test first
	private static readonly ResourceId[] StarterDeck =
	[
		ResourceId.Vanilla("cards/john"),
		ResourceId.Vanilla("cards/elbert"),
		ResourceId.Vanilla("cards/nicholas"),
		ResourceId.Vanilla("cards/john"),
		ResourceId.Vanilla("cards/john"),
	];

	public void Handle(Transaction context, PlayerJoinedEvent request)
	{
		var batch = new BlockBatch([]);

		// Create new deck
		var deckId = EntityId.New();
		batch.Blocks.Add(new InstantiateDeckBlock(deckId));
		batch.Blocks.Add(new AttachDeckToPlayerBlock(request.PlayerId, deckId));
		
		// Populate deck with card
		foreach (var defId in StarterDeck)
		{
			var def = context.Registry.CardContent.Get(defId);
			var cardId = EntityId.New();
			batch.Blocks.Add(new InstantiateCardBlock(cardId));
			batch.Blocks.Add(new AttachCardToDeckBlock(deckId, cardId));

			// TODO: definition to load
		}

		context.ApplyBlockBatch(batch);
	}
}