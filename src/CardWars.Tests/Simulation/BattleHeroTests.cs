using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Tags;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleHeroTests
{
	[Fact]
	public void PlayerJoin_CreatesAndAttachesIlyasForBothPlayers()
	{
		var battle = BattleTestHarness.Create();

		var hero = battle.HeroOf(battle.Player1Id);
		var opponentHero = battle.HeroOf(battle.Player2Id);

		Assert.True(hero.IsHero);
		Assert.Equal("Ilyas", hero.Name);
		Assert.Equal(10, hero.Hrt);
		Assert.Equal(battle.BattlefieldOf(battle.Player1Id).Id, hero.OwnerBattlefieldId);
		Assert.Equal(opponentHero.OwnerBattlefieldId, battle.BattlefieldOf(battle.Player2Id).Id);
		Assert.DoesNotContain(hero.Id, battle.Player1.HandCardIds);
		Assert.DoesNotContain(hero.Id, battle.DeckOf(battle.Player1Id).CardIds);
	}

	[Fact]
	public void Hero_CannotBeAttackedWhileAUnitIsAlive()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var blocker = battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var hero = battle.HeroOf(battle.Player2Id);
		var heroHrt = hero.Hrt;
		battle.AdvanceToAttackingPhase();

		battle.AttackHero(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			hero);

		Assert.Equal(heroHrt, hero.Hrt);
		Assert.Equal(1, attacker.Charge);

		blocker.Data.Set("hp", 0);

		battle.AttackHero(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			hero);

		Assert.Equal(heroHrt - GenericCard.AtkToHrt(attacker.Atk), hero.Hrt);
		Assert.Equal(0, attacker.Charge);
	}

	[Fact]
	public void HeroAttack_UsesOriginDamage()
	{
		var battle = BattleTestHarness.Create((id, definition) =>
		{
			if (id != TestCards.John) return;

			definition.Set("sp_atks", new ListTag()
				.Add(TestSupport.AttackPointer(new CompoundTag()
					.Set("name", "Origin Bonus")
					.Set("charge_cost", 0)
					.Set("patterns", new ListTag().Add(
						new CompoundTag().Set("type", "single").Set("value", 10))))));
		});

		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var hero = battle.HeroOf(battle.Player2Id);
		var heroHrt = hero.Hrt;
		battle.AdvanceToAttackingPhase();

		battle.AttackHero(
			battle.Player1Id,
			battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)),
			hero);

		Assert.Equal(heroHrt - GenericCard.AtkToHrt(attacker.Atk + 10), hero.Hrt);
	}

	[Fact]
	public void HeroAttack_RejectsAttackerOwnedByAnotherPlayer()
	{
		var battle = BattleTestHarness.Create();
		battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		battle.DrawAndPlay(battle.Player2Id, new UnitSlotPos(0, 0));
		var hero = battle.HeroOf(battle.Player1Id);
		var heroHrt = hero.Hrt;
		battle.AdvanceToAttackingPhase();

		battle.Engine.HandleInput(battle.Player2Id, new AttackHeroRequestInput
		{
			AttackerSlotIds = [battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0)).Id],
			HeroCardId = hero.Id
		});

		Assert.Equal(heroHrt, hero.Hrt);
	}
}
