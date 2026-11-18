using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleSpAttackTests
{
	[Fact]
	public void SpAtk_UsesHighestDamageSpecAcrossPatterns()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.By) return;

			definition.Set("charge", 2);
			definition.Set("sp_atks", new ListTag()
				.Add(SpAtk("Flat", 1, false, Pattern(AttackPatternType.Cell, 1)))
				.Add(SpAtk("Percent", 1, false, Pattern(AttackPatternType.Cell, 0, percent: 0.5f))));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var target = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(targetHp - 2, target.Hp);
		Assert.Equal(0, attacker.Charge);
	}

	[Fact]
	public void SpAtk_WithInsufficientCombinedCharge_DoesNotFallBackToBaAtk()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.By) return;

			definition.Set("charge", 1);
			definition.Set("ba_atk", BaAtk(Pattern(AttackPatternType.Cell, 100)));
			definition.Set("sp_atks", new ListTag()
				.Add(SpAtk("First", 1, false, Pattern(AttackPatternType.Cell, 1)))
				.Add(SpAtk("Second", 1, false, Pattern(AttackPatternType.Cell, 1))));
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
			if (id != TestCards.By) return;

			definition.Set("sp_atks", new ListTag()
				.Add(SpAtk("Pierce", 0, true, Pattern(AttackPatternType.Cell, 1))));
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

		Assert.Equal(targetHp - 1, target.Hp);
	}

	[Fact]
	public void BaAtk_CustomPattern_UsesDamageSpec()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.By) return;

			definition.Set("ba_atk", BaAtk(Pattern(AttackPatternType.Row, 2)));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var target = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var targetHp = target.Hp;
		battle.AdvanceToAttackingPhase();

		battle.Attack(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			battle.SlotOf(battle.Player2Id, new UnitSlotPos(0, 0)));

		Assert.Equal(targetHp - 2, target.Hp);
	}

	[Fact]
	public void AttackProfile_UsesHighestOverlappingDamageSpec()
	{
		var attack = new AttackProfile(
			true,
			new AttackDefinition(
				[
					Pattern(AttackPatternType.Row, 1),
					Pattern(AttackPatternType.Column, 2),
					Pattern(AttackPatternType.All, 3),
					Pattern(AttackPatternType.Cell, 4, 0, -1)
				],
				AttackDefinition.DefaultBasic.Texture,
				"Test", 
				1,
				true
			)
		);

		Assert.True(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(0, 0),
			out var center));
		Assert.Equal(3, center.Amount);

		Assert.True(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(0, -1),
			out var backRow));
		Assert.Equal(4, backRow.Amount);

		Assert.True(attack.IsHit(
			new UnitSlotPos(0, 0),
			new UnitSlotPos(-1, -1),
			out var all));
		Assert.Equal(3, all.Amount);
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

	private static CompoundTag SpAtk(
		string name,
		int chargeCost,
		bool bypass,
		params AttackPattern[] patterns)
		=> TestSupport.AttackPointer(DataTagMapper.ToTag(new AttackDefinition(patterns, ResourceId.Vanilla("texture/icon/attacks/test"), name, chargeCost, bypass)));

	private static CompoundTag BaAtk(params AttackPattern[] patterns)
		=> TestSupport.AttackPointer(DataTagMapper.ToTag(new AttackDefinition(patterns, ResourceId.Vanilla("texture/icon/attacks/test"), "Test BA", 1, false)));

	private static AttackPattern Pattern(
		AttackPatternType type,
		int amount,
		int x = 0,
		int y = 0,
		float percent = 0f)
		=> new(type, new UnitSlotPos(x, y), new AttackDamageSpec(percent, amount));
}