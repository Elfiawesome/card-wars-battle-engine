using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Tags;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleSpAttackTests
{
	[Fact]
	public void SpAtk_CombinesAllPatternsOnTopOfBaseAtk()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.John) return;

			definition.Set("charge", 2);
			definition.Set("sp_atk", new ListTag()
				.Add(SpAtk("Flat", 1, false, Pattern("single", 1)))
				.Add(SpAtk("Percent", 1, false, Pattern("single", "50%"))));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var target = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(targetHp - (attacker.Atk + 1 + attacker.Atk / 2), target.Hp);
		Assert.Equal(0, attacker.Charge);
	}

	[Fact]
	public void SpAtk_WithInsufficientCombinedCharge_DoesNotFallBackToBaAtk()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.John) return;

			definition.Set("charge", 1);
			definition.Set("ba_atk", new CompoundTag()
				.Set("charge_cost", 1)
				.Set("patterns", new ListTag().Add(Pattern("single", 100))));
			definition.Set("sp_atk", new ListTag()
				.Add(SpAtk("First", 1, false, Pattern("single", 1)))
				.Add(SpAtk("Second", 1, false, Pattern("single", 1))));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var target = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(targetHp, target.Hp);
		Assert.Equal(1, attacker.Charge);
	}

	[Fact]
	public void SpAtk_WithBypass_CanHitProtectedBackRow()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.John) return;

			definition.Set("sp_atk", new ListTag()
				.Add(SpAtk("Pierce", 0, true, Pattern("single", 1))));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var p2Cards = battle.DrawCards(battle.Player2Id, 2);
		battle.PlayCard(battle.Player2Id, p2Cards[0], battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		battle.PlayCard(battle.Player2Id, p2Cards[1], battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1)));
		var target = battle.CardIn(battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1)));
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1)));

		Assert.Equal(targetHp - (attacker.Atk + 1), target.Hp);
	}

	[Fact]
	public void BaAtk_CustomPattern_AddsDamage()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.John) return;

			definition.Set("ba_atk", new CompoundTag()
				.Set("charge_cost", 1)
				.Set("patterns", new ListTag().Add(Pattern("row", 2))));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var target = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(targetHp - (attacker.Atk + 2), target.Hp);
	}

	[Fact]
	public void DamageResolver_ExpandsPatternsAndSumsOverlaps()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));

		var p2Cards = battle.DrawCards(battle.Player2Id, 4);
		battle.PlayCard(battle.Player2Id, p2Cards[0], battle.SlotOf(battle.Player2Id, new UnitSlotPos(-1, 0)));
		battle.PlayCard(battle.Player2Id, p2Cards[1], battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));
		battle.PlayCard(battle.Player2Id, p2Cards[2], battle.SlotOf(battle.Player2Id, new UnitSlotPos(1, 0)));
		battle.PlayCard(battle.Player2Id, p2Cards[3], battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1)));

		var origin = battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0));
		var attack = new AttackProfile("Test", 1, true,
		[
			new AttackPattern(AttackPatternType.Row, 0, 0, new AttackDamageSpec(false, 1)),
			new AttackPattern(AttackPatternType.Column, 0, 0, new AttackDamageSpec(false, 2)),
			new AttackPattern(AttackPatternType.All, 0, 0, new AttackDamageSpec(false, 3)),
			new AttackPattern(AttackPatternType.Cell, 0, -1, new AttackDamageSpec(false, 4))
		], true);

		var damage = AttackDamageResolver.Resolve(battle.State, attacker, attack, origin.Id);

		Assert.Equal(attacker.Atk + 1 + 2 + 3, damage[battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)).Id]);
		Assert.Equal(1 + 3, damage[battle.SlotOf(battle.Player2Id, new UnitSlotPos(-1, 0)).Id]);
		Assert.Equal(1 + 3, damage[battle.SlotOf(battle.Player2Id, new UnitSlotPos(1, 0)).Id]);
		Assert.Equal(2 + 3 + 4, damage[battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, -1)).Id]);
	}

	[Fact]
	public void Attack_RejectsAttackerOwnedByAnotherPlayer()
	{
		var battle = BattleTestHarness.Create();
		var p1Target = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var p1TargetHp = p1Target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Engine.HandleInput(battle.Player1Id, new AttackRequestInput
		{
			AttackerSlotIds = [battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)).Id],
			TargetSlotId = battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)).Id
		});

		Assert.Equal(p1TargetHp, p1Target.Hp);
	}

	private static CompoundTag SpAtk(string name, int chargeCost, bool bypass, params CompoundTag[] patterns)
		=> new CompoundTag()
			.Set("name", name)
			.Set("charge_cost", chargeCost)
			.Set("bypass", bypass)
			.Set("patterns", List(patterns));

	private static ListTag List(params CompoundTag[] patterns)
	{
		var list = new ListTag();
		foreach (var pattern in patterns) list.Add(pattern);
		return list;
	}

	private static CompoundTag Pattern(string type, int value)
		=> new CompoundTag().Set("type", type).Set("value", value);

	private static CompoundTag Pattern(string type, string value)
		=> new CompoundTag().Set("type", type).Set("value", value);
}
