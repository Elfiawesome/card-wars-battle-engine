using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattlePtTests
{
	[Fact]
	public void PlayerJoin_StartsWithRoundOnePt()
	{
		var battle = BattleTestHarness.Create();

		Assert.Equal(5, battle.Player1.Pt);
		Assert.Equal(5, battle.Player2.Pt);
	}

	[Fact]
	public void PlayingCard_SpendsCardPt()
	{
		var battle = BattleTestHarness.Create();
		var card = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));

		Assert.Equal(5 - card.Pt, battle.Player1.Pt);
	}

	[Fact]
	public void InsufficientPt_RejectsCardPlay()
	{
		var battle = BattleTestHarness.Create();
		battle.Player1.Pt = 0;
		var card = battle.DrawCard(battle.Player1Id);
		var slot = battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0));

		battle.PlayCard(battle.Player1Id, card, slot);

		Assert.Null(slot.HoldingCardId);
		Assert.Contains(card.Id, battle.Player1.HandCardIds);
		Assert.Equal(0, battle.Player1.Pt);
	}

	[Fact]
	public void InvalidTarget_DoesNotSpendPt()
	{
		var battle = BattleTestHarness.Create();
		var card = battle.DrawCard(battle.Player1Id);

		battle.Engine.HandleInput(battle.Player1Id, new UseCardRequestInput
		{
			CardId = card.Id,
			TargetEntityId = null
		});

		Assert.Equal(5, battle.Player1.Pt);
		Assert.Contains(card.Id, battle.Player1.HandCardIds);
	}

	[Fact]
	public void RoundEnd_TopsUpPtToNextRoundTarget()
	{
		var battle = BattleTestHarness.Create();
		var card = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));

		Assert.Equal(5 - card.Pt, battle.Player1.Pt);

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.Equal(6, battle.Player1.Pt);
		Assert.Equal(6, battle.Player2.Pt);
	}

	[Fact]
	public void RoundEnd_DoesNotReducePtAboveTarget()
	{
		var battle = BattleTestHarness.Create();
		battle.Player1.Pt = 10;

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.Equal(10, battle.Player1.Pt);
	}
}
