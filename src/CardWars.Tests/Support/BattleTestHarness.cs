using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.Tests.Support;

internal sealed class BattleTestHarness
{
	public BattleEngine.BattleEngine Engine { get; }
	public GameState State => Engine.State;
	public EntityId Player1Id { get; }
	public EntityId Player2Id { get; }

	public Player Player1 => Require<Player>(Player1Id);
	public Player Player2 => Require<Player>(Player2Id);

	private BattleTestHarness(
		BattleEngine.BattleEngine engine,
		EntityId player1Id,
		EntityId player2Id)
	{
		Engine = engine;
		Player1Id = player1Id;
		Player2Id = player2Id;
	}

	public static BattleTestHarness Create(Action<ResourceId, CompoundTag>? configureCards = null)
	{
		var registry = TestSupport.CreateRegistry();
		TestCards.RegisterStarterDeck(registry, configureCards);

		var engine = new BattleEngine.BattleEngine { Registry = registry };
		var player1Id = EntityId.New();
		var player2Id = EntityId.New();

		engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(player1Id, 1));
		engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(player2Id, 2));

		return new BattleTestHarness(engine, player1Id, player2Id);
	}

	public Player Player(EntityId playerId)
		=> Require<Player>(playerId);

	public Deck DeckOf(EntityId playerId)
		=> Require<Deck>(Player(playerId).DeckIds.Single());

	public Battlefield BattlefieldOf(EntityId playerId)
		=> Require<Battlefield>(Player(playerId).BattlefieldIds.Single());

	public IReadOnlyList<UnitSlot> SlotsOf(EntityId playerId)
		=> BattlefieldOf(playerId).UnitSlotIds
			.Select(id => State.Get<UnitSlot>(id))
			.OfType<UnitSlot>()
			.ToList();

	public UnitSlot SlotOf(EntityId playerId, UnitSlotPos position)
		=> SlotsOf(playerId).Single(slot => slot.Position == position);

	public GenericCard CardIn(UnitSlot slot)
		=> slot.HoldingCardId is { } cardId && State.Get<GenericCard>(cardId) is { } card
			? card
			: throw new InvalidOperationException($"Slot [{slot.Id}] has no card");

	public IReadOnlyList<GenericCard> HandOf(EntityId playerId)
		=> Player(playerId).HandCardIds
			.Select(id => State.Get<GenericCard>(id))
			.OfType<GenericCard>()
			.ToList();

	public IReadOnlyList<GenericCard> HeroesOf(EntityId playerId)
		=> BattlefieldOf(playerId).HeroCardIds
			.Select(id => State.Get<GenericCard>(id))
			.OfType<GenericCard>()
			.ToList();

	public IReadOnlyList<GenericCard> GraveyardOf(EntityId playerId)
		=> Player(playerId).GraveyardCardIds
			.Select(id => State.Get<GenericCard>(id))
			.OfType<GenericCard>()
			.ToList();

	public GenericCard HeroOf(EntityId playerId)
		=> HeroesOf(playerId).Single();

	public GenericCard DrawCard(EntityId playerId)
	{
		var deck = DeckOf(playerId);
		var cardId = deck.CardIds.FirstOrDefault();
		if (cardId.IsNone) throw new InvalidOperationException($"Deck [{deck.Id}] is empty");

		Engine.HandleInput(EntityId.None, new DrawCardRequestInput(deck.Id, playerId));
		return State.Require<GenericCard>(cardId)
			?? throw new InvalidOperationException($"Drawn card [{cardId}] was not found");
	}

	public IReadOnlyList<GenericCard> DrawCards(EntityId playerId, int count)
		=> Enumerable.Range(0, count).Select(_ => DrawCard(playerId)).ToList();

	public void PlayCard(EntityId playerId, GenericCard card, UnitSlot slot)
	{
		State.Turn.AllowedPlayerInputs.Add(playerId);
		Engine.HandleInput(playerId, new UseCardRequestInput
		{
			CardId = card.Id,
			TargetEntityId = slot.Id
		});
	}

	public GenericCard DrawAndPlay(EntityId playerId, UnitSlotPos position)
	{
		var card = DrawCard(playerId);
		PlayCard(playerId, card, SlotOf(playerId, position));
		return card;
	}

	public void EndTurn(EntityId playerId)
		=> Engine.HandleInput(playerId, new EndTurnRequestInput());

	public void AdvanceToAttackingPhase()
	{
		EndTurn(Player1Id);
		EndTurn(Player2Id);
	}

	public void Attack(EntityId playerId, UnitSlot attackerSlot, UnitSlot targetSlot)
		=> Attack(playerId, [attackerSlot], targetSlot);

	public void Attack(EntityId playerId, IReadOnlyCollection<UnitSlot> attackerSlots, UnitSlot targetSlot)
		=> Engine.HandleInput(playerId, new AttackRequestInput
		{
			AttackerSlotIds = [.. attackerSlots.Select(slot => slot.Id)],
			TargetSlotId = targetSlot.Id
		});

	public void AttackHero(EntityId playerId, UnitSlot attackerSlot, GenericCard hero)
		=> Engine.HandleInput(playerId, new AttackHeroRequestInput
		{
			AttackerSlotIds = [attackerSlot.Id],
			HeroCardId = hero.Id
		});

	public void AttackHero(EntityId playerId, IReadOnlyCollection<UnitSlot> attackerSlots, GenericCard hero)
		=> Engine.HandleInput(playerId, new AttackHeroRequestInput
		{
			AttackerSlotIds = [.. attackerSlots.Select(slot => slot.Id)],
			HeroCardId = hero.Id
		});

	private T Require<T>(EntityId id) where T : class, IEntity
		=> State.Require<T>(id)
		   ?? throw new InvalidOperationException($"Entity [{id}] is not {typeof(T).Name}");
}
