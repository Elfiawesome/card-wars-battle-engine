using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class PlayerJoinedEvent() : IEvent
{
	[DataTag] public EntityId PlayerId { get; set; }
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
			var def = context.Registry.CardDefinitions.Get(defId);
			var cardId = EntityId.New();
			batch.Blocks.Add(new InstantiateCardBlock(cardId, def));
			batch.Blocks.Add(new AttachCardToDeckBlock(deckId, cardId));
		}

		// Create new Battlefield
		var battlefieldId = EntityId.New();
		batch.Blocks.Add(new InstantiateBattlefieldBlock(battlefieldId));
		batch.Blocks.Add(new AttachBattlefieldToPlayerBlock(request.PlayerId, battlefieldId));
		
		// Create a 4 unit sot
		for (int i = 0; i < 3; i++)
		{
			var unitSlotId = EntityId.New();
			batch.Blocks.Add(new InstantiateUnitSlotBlock(unitSlotId));
			batch.Blocks.Add(new AttachUnitSlotToBattlefieldBlock(battlefieldId, unitSlotId));
		}

		context.ApplyBlockBatch(batch);
	}
}