using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Simulation;

public sealed class BattleWinConditionTests
{
	[Fact]
	public void RoundEnd_DefeatedPlayerLosesBoardAndTeamLoses()
	{
		var battle = BattleTestHarness.Create();
		var hero = battle.HeroOf(battle.Player1Id);
		var battlefieldId = battle.BattlefieldOf(battle.Player1Id).Id;
		hero.Data.Set("hrt", -3);

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.True(battle.Player1.IsDefeated);
		Assert.Contains(hero.Id, battle.Player1.DefeatedHeroIds);
		Assert.Equal(-3, hero.Hrt);
		Assert.Null(battle.State.Get<Battlefield>(battlefieldId));
		Assert.Null(battle.State.Get<GenericCard>(hero.Id));
		Assert.Empty(battle.Player1.BattlefieldIds);
		Assert.Empty(battle.GraveyardOf(battle.Player1Id));
		Assert.True(battle.State.IsGameOver);
		Assert.False(battle.State.IsDraw);
		Assert.Equal(2, battle.State.WinningTeam);
	}

	[Fact]
	public void DefeatedPlayer_UnitsAndSlotsAreDestroyed()
	{
		var battle = BattleTestHarness.Create();
		var unit = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var slot = battle.SlotOf(battle.Player1Id, new UnitSlotPos(0, 0));
		var battlefieldId = battle.BattlefieldOf(battle.Player1Id).Id;
		battle.HeroOf(battle.Player1Id).Data.Set("hrt", 0);

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.Contains(unit.Id, battle.Player1.GraveyardCardIds);
		Assert.NotNull(battle.State.Get<GenericCard>(unit.Id));
		Assert.Null(battle.State.Get<UnitSlot>(slot.Id));
		Assert.Null(battle.State.Get<Battlefield>(battlefieldId));
		Assert.Empty(battle.Player1.BattlefieldIds);
		Assert.True(battle.State.IsGameOver);
		Assert.Equal(2, battle.State.WinningTeam);
	}

	[Fact]
	public void DefeatedHero_IsNotTargetable()
	{
		var battle = BattleTestHarness.Create();
		var attacker = battle.DrawAndPlay(battle.Player1Id, new UnitSlotPos(0, 0));
		var hero = battle.HeroOf(battle.Player2Id);
		hero.Data.Set("hrt", 0);

		Assert.False(HeroTargetResolver.CanTarget(
			battle.State,
			attacker,
			hero.Id,
			battle.Engine.Registry.DataDriven));
	}

	[Fact]
	public void HeroLoseCondition_CanBeOverriddenByData()
	{
		var battle = BattleTestHarness.Create();
		var hero = battle.HeroOf(battle.Player2Id);
		hero.Data.Set("lose_condition", DataTagMapper.ToTag(
			new LiteralExpression(new BoolTag(true))));

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.True(battle.Player2.IsDefeated);
		Assert.Equal(1, battle.State.WinningTeam);
	}

	[Fact]
	public void HeroLoseCondition_EvaluatesSelfData()
	{
		var battle = BattleTestHarness.Create();
		var hero = battle.HeroOf(battle.Player2Id);
		hero.Data.Set("lose_condition", DataTagMapper.ToTag(
			new GreaterThanExpression(
				new MemberExpression(
					new VariableExpression(ScriptBindings.Self),
					"hrt"),
				new LiteralExpression(new IntTag(5)))));

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.True(battle.Player2.IsDefeated);
		Assert.Equal(1, battle.State.WinningTeam);
	}

	[Fact]
	public void RoundEnd_WithNoLivingHeroes_IsDraw()
	{
		var battle = BattleTestHarness.Create();
		battle.HeroOf(battle.Player1Id).Data.Set("hrt", 0);
		battle.HeroOf(battle.Player2Id).Data.Set("hrt", 0);

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.True(battle.State.IsGameOver);
		Assert.True(battle.State.IsDraw);
		Assert.Null(battle.State.WinningTeam);
	}

	[Fact]
	public void DefeatedPlayer_IsRemovedFromTurnOrderWhenTeamStillAlive()
	{
		var battle = BattleTestHarness.Create();
		var player3Id = EntityId.New();
		battle.Engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(player3Id, 1));
		battle.HeroOf(battle.Player1Id).Data.Set("hrt", 0);

		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);
		battle.EndTurn(player3Id);
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);
		battle.EndTurn(player3Id);

		Assert.True(battle.Player1.IsDefeated);
		Assert.False(battle.State.IsGameOver);
		Assert.DoesNotContain(battle.Player1Id, battle.State.Turn.TurnOrder);
		Assert.Contains(player3Id, battle.State.Turn.TurnOrder);
	}

	[Fact]
	public void CustomWinPredicate_CanOverrideDefaultWinner()
	{
		var battle = BattleTestHarness.Create();
		var ruleId = ResourceId.Vanilla("rules/team_two_wins");
		var rule = new GameRulesDefinition
		{
			PtSchedule = [5, 6],
			PtCap = 50,
			WinCondition = new EqualsExpression(
				new VariableExpression(WinBindings.Team),
				new LiteralExpression(new IntTag(2)))
		};

		battle.Engine.Registry.GameRules.Register(ruleId, rule);
		battle.State.RuleId = ruleId;

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		Assert.True(battle.State.IsGameOver);
		Assert.False(battle.State.IsDraw);
		Assert.Equal(2, battle.State.WinningTeam);
	}

	[Fact]
	public void GameOver_BlocksFurtherInput()
	{
		var battle = BattleTestHarness.Create();
		battle.HeroOf(battle.Player1Id).Data.Set("hrt", 0);

		battle.AdvanceToAttackingPhase();
		battle.EndTurn(battle.Player1Id);
		battle.EndTurn(battle.Player2Id);

		var turn = battle.State.Turn.Clone();
		battle.EndTurn(battle.Player2Id);

		Assert.True(battle.State.Turn.Equals(turn));
	}
}
