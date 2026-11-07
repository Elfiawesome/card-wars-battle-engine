using CardWars.BattleEngine;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla;
using CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Tags;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using CardWars.Tests.Support;
using Xunit;

namespace CardWars.Tests.Vanilla;

public sealed class GameRulesTests
{
	[Fact]
	public void Content_LoadsPtScheduleAndWinPredicate()
	{
		var provider = new MemoryFileProvider();
		provider.AddFile("rules/default.json", """
			{
				"_type": "cardwars:battle/rules",
				"pt_schedule": [5, 6],
				"pt_cap": 50,
				"win_condition": {
					"_type": "cardwars:script/expression/and",
					"left": {
						"_type": "cardwars:script/expression/variable",
						"name": "cardwars:win/team_alive"
					},
					"right": {
						"_type": "cardwars:script/expression/equals",
						"left": {
							"_type": "cardwars:script/expression/variable",
							"name": "cardwars:win/alive_team_count"
						},
						"right": {
							"_type": "cardwars:script/expression/literal",
							"value": 1
						}
					}
				}
			}
			""");

		var registry = new BattleEngineRegistry();
		var content = new ModContentResult
		{
			Id = GameRulesDefinition.DefaultId,
			FilePath = new StoragePath("rules/default.json", provider),
			Category = ["rules"]
		};

		new VanillaMod().OnLoad(registry, [content]);

		var rules = GameRulesDefinition.Get(registry, GameRulesDefinition.DefaultId)
			?? throw new InvalidOperationException("Default rules were not registered");

		Assert.Equal(new[] { 5, 6 }, rules.PtSchedule);
		Assert.Equal(50, rules.PtCap);
		var predicate = Assert.IsType<AndExpression>(rules.WinCondition);

		var context = new ScriptContext(new GameState(), registry.DataDriven);
		context.DeclareVariable(WinBindings.Team, new IntTag(1));
		context.DeclareVariable(WinBindings.TeamAlive, new BoolTag(true));
		context.DeclareVariable(WinBindings.AliveTeamCount, new IntTag(1));
		context.DeclareVariable(WinBindings.AlivePlayerCount, new IntTag(1));
		context.DeclareVariable(WinBindings.AliveTeams, new ListTag().Add(1));

		Assert.True(context.Evaluate(predicate).IsTruthy);
	}

	[Fact]
	public void PtSchedule_AdvancesAndCaps()
	{
		var rules = new GameRulesDefinition
		{
			PtSchedule = [5, 6, 8],
			PtCap = 7
		};

		Assert.Equal(5, rules.GetPtForRound(1));
		Assert.Equal(6, rules.GetPtForRound(2));
		Assert.Equal(7, rules.GetPtForRound(3));
		Assert.Equal(7, rules.GetPtForRound(20));
	}
}
