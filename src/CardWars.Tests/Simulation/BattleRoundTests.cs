using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleRoundTests
{
	[Fact]
	public void PhaseEnd_DoesNotRefillCharge()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		attacker.Data.Set("charge", 0);

		battle.AdvanceToAttackingPhase();

		Assert.Equal(0, attacker.Charge);
	}

	[Fact]
	public void TurnEnd_BeforeRoundEnd_DoesNotRefillCharge()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(0, attacker.Charge);

		battle.EndTurn(battle.Player1Id);

		Assert.Equal(0, attacker.Charge);
	}

	[Fact]
	public void RoundEnd_RefillsEveryDeployedUnit()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var target = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		var targetCharge = target.Charge;
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.Equal(attacker.ChargeMax, attacker.Charge);
		Assert.Equal(target.ChargeMax, target.Charge);
		Assert.Equal(target.ChargeMax, targetCharge);
	}
}
