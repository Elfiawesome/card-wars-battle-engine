using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleSetupTests
{
	[Fact]
	public void PlayerJoin_CreatesDeckBattlefieldAndSlots()
	{
		var battle = BattleTestHarness.Create();
		var player = battle.Player1;

		Assert.Single(player.DeckIds);
		Assert.Single(player.BattlefieldIds);
		Assert.Empty(player.HandCardIds);
		Assert.Equal(5, battle.DeckOf(player.Id).CardIds.Count);

		var slots = battle.SlotsOf(player.Id);
		Assert.Equal(4, slots.Count);
		Assert.Contains(slots, slot => slot.Position == new UnitSlotPos(-1, 0));
		Assert.Contains(slots, slot => slot.Position == new UnitSlotPos(0, 0));
		Assert.Contains(slots, slot => slot.Position == new UnitSlotPos(1, 0));
		Assert.Contains(slots, slot => slot.Position == new UnitSlotPos(0, -1));
	}

	[Fact]
	public void DrawCard_MovesCardFromDeckToHand()
	{
		var battle = BattleTestHarness.Create();
		var deck = battle.DeckOf(battle.Player1Id);
		var cardId = deck.CardIds[0];

		var card = battle.DrawCard(battle.Player1Id);

		Assert.Equal(cardId, card.Id);
		Assert.DoesNotContain(cardId, deck.CardIds);
		Assert.Contains(cardId, battle.Player1.HandCardIds);
	}

	[Fact]
	public void PlayUnitCard_AttachesToEmptySlot()
	{
		var battle = BattleTestHarness.Create();
		var card = battle.DrawCard(battle.Player1Id);
		var slot = battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0));

		battle.PlayCard(battle.Player1Id, card, slot);

		Assert.Equal(card.Id, slot.HoldingCardId);
		Assert.Equal(slot.Id, card.OwnerUnitSlotId);
		Assert.DoesNotContain(card.Id, battle.Player1.HandCardIds);
	}

	[Fact]
	public void PlayUnitCard_DoesNotOverwriteOccupiedSlot()
	{
		var battle = BattleTestHarness.Create();
		var first = battle.DrawCard(battle.Player1Id);
		var slot = battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0));
		battle.PlayCard(battle.Player1Id, first, slot);

		var second = battle.DrawCard(battle.Player1Id);
		battle.PlayCard(battle.Player1Id, second, slot);

		Assert.Equal(first.Id, slot.HoldingCardId);
		Assert.Contains(second.Id, battle.Player1.HandCardIds);
	}

	[Fact]
	public void TurnFlow_AdvancesPlayersAndTogglesPhase()
	{
		var battle = BattleTestHarness.Create();
		var turn = battle.State.Turn;

		Assert.Equal(TurnPhase.Setup, turn.Phase);
		Assert.Equal(1, turn.RoundNumber);
		Assert.Equal(0, turn.TurnIndex);
		Assert.Contains(battle.Player1Id, turn.AllowedPlayerInputs);

		battle.EndTurn(battle.Player1Id);
		turn = battle.State.Turn;
		Assert.Equal(1, turn.TurnIndex);
		Assert.Contains(battle.Player2Id, turn.AllowedPlayerInputs);

		battle.EndTurn(battle.Player2Id);
		turn = battle.State.Turn;
		Assert.Equal(0, turn.TurnIndex);
		Assert.Equal(1, turn.TurnNumber);
		Assert.Equal(1, turn.RoundNumber);
		Assert.Equal(TurnPhase.Attacking, turn.Phase);
		Assert.Contains(battle.Player1Id, turn.AllowedPlayerInputs);

		battle.EndTurn(battle.Player1Id);
		turn = battle.State.Turn;
		Assert.Equal(1, turn.TurnIndex);
		Assert.Contains(battle.Player2Id, turn.AllowedPlayerInputs);

		battle.EndTurn(battle.Player2Id);
		turn = battle.State.Turn;
		Assert.Equal(0, turn.TurnIndex);
		Assert.Equal(2, turn.TurnNumber);
		Assert.Equal(2, turn.RoundNumber);
		Assert.Equal(TurnPhase.Setup, turn.Phase);
		Assert.Contains(battle.Player1Id, turn.AllowedPlayerInputs);
	}

	[Fact]
	public void NonAllowedPlayerInput_IsIgnored()
	{
		var battle = BattleTestHarness.Create();
		var handCount = battle.Player2.HandCardIds.Count;
		var deck = battle.DeckOf(battle.Player2Id);

		battle.Engine.HandleInput(
			battle.Player2Id,
			new DrawCardRequestInput(deck.Id, battle.Player2Id));

		Assert.Equal(handCount, battle.Player2.HandCardIds.Count);
		Assert.Equal(5, deck.CardIds.Count);
	}

	[Fact]
	public void NonAllowedPlayer_CannotPlayCard()
	{
		var battle = BattleTestHarness.Create();
		var card = battle.DrawCard(battle.Player2Id);
		var slot = battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0));

		battle.Engine.HandleInput(battle.Player2Id, new UseCardRequestInput
		{
			CardId = card.Id,
			TargetEntityId = slot.Id
		});

		Assert.Null(slot.HoldingCardId);
		Assert.Contains(card.Id, battle.Player2.HandCardIds);
	}
}
