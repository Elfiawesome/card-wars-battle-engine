using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Logging;

namespace CardWars.Webserver;

public class BattleEngineSimulator
{
	public BattleEngine.BattleEngine Engine { get; init; } = new();
	public GameState State => Engine.State;

	public BattleEngineSimulator()
	{
		Engine.LoadMod(new VanillaMod());
	}

	public EntityId AddPlayer()
	{
		var playerId = new EntityId(Guid.NewGuid());
		Engine.HandleInput(Guid.Empty, new PlayerJoinedRequestInput(playerId));
		return playerId;
	}

	public void DrawCard(EntityId playerId, EntityId DeckId)
	{
		Engine.HandleInput(playerId, new DrawCardRequestInput() { DeckId = DeckId, ReceivedPlayerId = playerId });
	}

	public IEnumerable<GenericCard> ListHandCards(EntityId playerId)
	{
		var player = State.Get<Player>(playerId) ?? throw new Exception($"No player {playerId} found!");
		foreach (var cardId in player.HandCardIds)
		{
			var card = State.Get<GenericCard>(cardId);
			if (card == null) { continue; }
			yield return card;
		}
	}

	public IEnumerable<Deck> ListDecks(EntityId playerId)
	{
		var player = State.Get<Player>(playerId) ?? throw new Exception($"No player {playerId} found!");
		foreach (var deckId in player.DeckIds)
		{
			var deck = State.Get<Deck>(deckId);
			if (deck == null) { continue; }
			yield return deck;
		}
	}

	public IEnumerable<Battlefield> ListBattlefields(EntityId playerId)
	{
		var player = State.Get<Player>(playerId) ?? throw new Exception($"No player {playerId} found!");
		foreach (var battlefieldId in player.BattlefieldIds)
		{
			var battlefield = State.Get<Battlefield>(battlefieldId);
			if (battlefield == null) { continue; }
			yield return battlefield;
		}
	}

	public IEnumerable<UnitSlot> ListUnitSlots(EntityId battlefieldId)
	{
		var battlefield = State.Get<Battlefield>(battlefieldId) ?? throw new Exception($"No battlefield {battlefieldId} found!");
		foreach (var unitSlotId in battlefield.UnitSlotIds)
		{
			var unitSlot = State.Get<UnitSlot>(unitSlotId);
			if (unitSlot == null) { continue; }
			yield return unitSlot;
		}
	}

	public void PlayCard(EntityId playerId, EntityId cardId, UnitSlotPos position = default, EntityId? battlefieldId = null)
	{
		if (battlefieldId == null)
			battlefieldId = ListBattlefields(playerId).First().Id;
		// Get Unit Slot
		var unitSlot = ListUnitSlots((EntityId)battlefieldId).FirstOrDefault((us) => us?.Position == position && us.HoldingCardId == null, null);
		if (unitSlot == null) { return; }

		PlayCard(playerId, cardId, unitSlot.Id);
	}

	public void PlayCard(EntityId playerId, EntityId cardId, EntityId? targetId = null)
	{
		Engine.HandleInput(playerId, new UseCardRequestInput() { CardId = cardId, TargetEntityId = targetId });
	}


	public void UnitAttack(EntityId playerId, EntityId attackerSlotId, UnitSlotPos position = default, EntityId? battlefieldId = null)
	{
		// What the hell, we need to have options for both slots to be position not some id some position smh...
		if (battlefieldId == null)
			battlefieldId = ListBattlefields(playerId).First().Id;
		// Get Unit Slot
		var unitSlot = ListUnitSlots((EntityId)battlefieldId).FirstOrDefault((us) => us?.Position == position, null);
		if (unitSlot == null) { return; }

		UnitAttack(playerId, attackerSlotId, unitSlot.Id);
	}

	public void UnitAttack(EntityId playerId, EntityId attackerSlotId, EntityId targetSlotId)
	{
		Engine.HandleInput(playerId, new AttackRequestInput() { AttackerSlotIds = [attackerSlotId], TargetSlotId = targetSlotId });
	}

	public void EndTurn(EntityId playerId)
	{
		Engine.HandleInput(playerId, new EndTurnRequestInput());
	}
}