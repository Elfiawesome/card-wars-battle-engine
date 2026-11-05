using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using Xunit;

namespace CardWars.Tests;

public sealed class BattleAttackTests
{
	[Fact]
	public void BasicAttack_DamagesTargetAndConsumesAttackerCharge()
	{
		var battle = BattleTestHarness.Create();
		battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var attacker = battle.CardIn(battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)));
		battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var target = battle.CardIn(battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		var attackerCharge = attacker.Charge;
		var targetCharge = target.Charge;
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(targetHp - attacker.Atk, target.Hp);
		Assert.Equal(attackerCharge - 1, attacker.Charge);
		Assert.Equal(targetCharge, target.Charge);
	}

	[Fact]
	public void Attack_OutsideAttackingPhase_DoesNothing()
	{
		var battle = BattleTestHarness.Create();
		battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var attacker = battle.CardIn(battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)));
		battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var target = battle.CardIn(battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		var hp = target.Hp;
		var charge = attacker.Charge;

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(hp, target.Hp);
		Assert.Equal(charge, attacker.Charge);
	}

	[Fact]
	public void Attack_WithNoCharge_DoesNoDamage()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id == TestCards.John) definition.Set("charge", 0);
		});
		battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var attacker = battle.CardIn(battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)));
		battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var target = battle.CardIn(battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		var hp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(hp, target.Hp);
		Assert.Equal(0, attacker.Charge);
	}

	[Fact]
	public void BackRow_IsProtectedUntilFrontRowIsFatal()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawCards(battle.Player1Id, 2)[1];
		battle.PlayCard(battle.Player1Id, attacker, battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)));

		var p2Cards = battle.DrawCards(battle.Player2Id, 3);
		var front = p2Cards[2];
		var back = p2Cards[0];
		battle.PlayCard(battle.Player2Id, front, battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		battle.PlayCard(battle.Player2Id, back, battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1)));
		battle.AdvanceToAttackingPhase();

		var frontSlot = battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0));
		var backSlot = battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1));

		battle.Attack(battle.Player1Id, battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)), backSlot);
		Assert.Equal(6, back.Hp);

		battle.Attack(battle.Player1Id, battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)), frontSlot);
		Assert.True(front.IsFatal);

		battle.Attack(battle.Player1Id, battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)), backSlot);
		Assert.Equal(1, back.Hp);
	}

	[Fact]
	public void MultipleAttackers_DamageTheSameTarget()
	{
		var battle = BattleTestHarness.Create();
		var p1Cards = battle.DrawCards(battle.Player1Id, 2);
		battle.PlayCard(battle.Player1Id, p1Cards[0], battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)));
		battle.PlayCard(battle.Player1Id, p1Cards[1], battle.SlotOf(battle.Player1Id, new UnitSlotPos(-1, 0)));

		var target = battle.DrawCard(battle.Player2Id);
		battle.PlayCard(battle.Player2Id, target, battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			[
				battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
				battle.SlotOf(battle.Player1Id, new UnitSlotPos(-1, 0))
			],
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(-3, target.Hp);
	}

	[Fact]
	public void Attack_EmptyEnemySlot_DoesNothing()
	{
		var battle = BattleTestHarness.Create();
		battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var attacker = battle.CardIn(battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)));
		var charge = attacker.Charge;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(charge, attacker.Charge);
		Assert.Null(battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)).HoldingCardId);
	}
}
