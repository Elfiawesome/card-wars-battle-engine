using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using Xunit;

namespace CardWars.Tests;

public sealed class CardPlayIntegrationTests
{
	[Fact]
	public void PlayerCanDrawAndPlayUnitCard()
	{
		var registry = TestSupport.CreateRegistry();
		registry.CardDefinitions.Register(
			ResourceId.Vanilla("cards/units/john"),
			new CompoundTag()
				.Set("name", "John")
				.Set("hp", 6)
				.Set("atk", 4)
				.Set("pt", 2)
				.Set("charge", 1)
				.Set("card_type", "unit")
				.Set("intrinsic_behaviours", new ListTag().Add(
					new CompoundTag().Set("resource", "cardwars:summon_unit_card_to_unit_slot_behaviour"))));

		var engine = new CardWars.BattleEngine.BattleEngine { Registry = registry };
		var playerId = EntityId.New();
		engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(playerId));

		var player = engine.State.Require<Player>(playerId);
		Assert.NotNull(player);
		var deck = engine.State.Require<Deck>(player.DeckIds.First());
		Assert.NotNull(deck);

		engine.HandleInput(playerId, new DrawCardRequestInput(deck.Id, playerId));

		var card = engine.State.Require<GenericCard>(player.HandCardIds.First());
		Assert.NotNull(card);
		var slot = engine.State.OfType<UnitSlot>().First(s => s.HoldingCardId is null);

		engine.HandleInput(playerId, new UseCardRequestInput
		{
			CardId = card.Id,
			TargetEntityId = slot.Id
		});

		Assert.Equal(card.Id, slot.HoldingCardId);
	}
}
