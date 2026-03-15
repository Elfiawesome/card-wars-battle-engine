using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
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
		ResourceId.Vanilla("cards/units/john"),
		ResourceId.Vanilla("cards/units/elbert"),
		ResourceId.Vanilla("cards/units/nicholas"),
		ResourceId.Vanilla("cards/units/john"),
		ResourceId.Vanilla("cards/units/john"),
	];

	public void Handle(Transaction context, PlayerJoinedEvent request)
	{
		// Make sure we don't re-create a player with the same id
		if (context.State.Require<Player>(request.PlayerId) is not { } player) return;

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

			// Add other data in a card that may not exist
			if (def != null)
			{
				switch (def.GetString("card_type"))
				{
					case "unit":
						if (!def.Has("hp_max")) def.Set("hp_max", def.GetInt("hp"));
						if (!def.Has("atk_max")) def.Set("atk_max", def.GetInt("atk"));
						if (!def.Has("pt_max")) def.Set("pt_max", def.GetInt("pt"));
						if (!def.Has("charge")) def.Set("charge", 1);
						if (!def.Has("charge_max")) def.Set("charge_max", def.GetInt("charge"));
						break;
					case "spell":
						break;
					case "hero":
						if (!def.Has("hrt")) def.Set("hrt", 0);
						if (!def.Has("hrt_max")) def.Set("hrt_max", def.GetInt("hrt"));
						break;
				}
			}

			batch.Blocks.Add(new InstantiateCardBlock(cardId, def));
			batch.Blocks.Add(new AttachCardToDeckBlock(deckId, cardId));
		}

		// Create new Battlefield
		var battlefieldId = EntityId.New();
		batch.Blocks.Add(new InstantiateBattlefieldBlock(battlefieldId));
		batch.Blocks.Add(new AttachBattlefieldToPlayerBlock(request.PlayerId, battlefieldId));


		var addUnitSlot = (BlockBatch b, EntityId bid, UnitSlotPos pos = default) =>
		{
			var usid = EntityId.New();
			b.Blocks.Add(new InstantiateUnitSlotBlock(usid));
			b.Blocks.Add(new ModifyUnitSlotPositionBlock(usid, pos));
			b.Blocks.Add(new AttachUnitSlotToBattlefieldBlock(bid, usid));
		};
		// Create a 4 unit slot (3 front row, 1 back unit)
		for (int i = 0; i < 3; i++)
		{
			addUnitSlot(batch, battlefieldId, new(-1 + i, 0));
		}
		addUnitSlot(batch, battlefieldId, new(0, -1));

		context.ApplyBlockBatch(batch);
	}
}