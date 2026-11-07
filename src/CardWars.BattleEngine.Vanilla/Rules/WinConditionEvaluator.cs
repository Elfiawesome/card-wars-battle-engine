using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Rules;

public static class WinBindings
{
	public static readonly ResourceId Team = ResourceId.Vanilla("win/team");
	public static readonly ResourceId TeamAlive = ResourceId.Vanilla("win/team_alive");
	public static readonly ResourceId AliveTeamCount = ResourceId.Vanilla("win/alive_team_count");
	public static readonly ResourceId AlivePlayerCount = ResourceId.Vanilla("win/alive_player_count");
	public static readonly ResourceId AliveTeams = ResourceId.Vanilla("win/alive_teams");
}

public readonly record struct WinConditionResult(IReadOnlyList<int> WinningTeams, bool IsDraw)
{
	public bool IsGameOver => WinningTeams.Count > 0 || IsDraw;
	public int? WinningTeam => WinningTeams.Count == 1 ? WinningTeams[0] : null;
}

public sealed class WinConditionEvaluator(BattleEngineRegistry registry)
{
	public WinConditionResult Evaluate(GameState state, GameRulesDefinition rules)
	{
		var players = state.OfType<Player>().ToList();
		var teams = players.Select(player => player.Team).Distinct().OrderBy(team => team).ToList();
		if (teams.Count == 0) return new([], false);

		var alivePlayers = players.Where(player => !player.IsDefeated).ToList();
		var aliveTeams = teams.Where(team => alivePlayers.Any(player => player.Team == team)).ToList();
		if (aliveTeams.Count == 0) return new([], true);

		if (rules.WinCondition is null)
			return aliveTeams.Count == 1
				? new([aliveTeams[0]], false)
				: new([], false);

		var aliveTeamsTag = new ListTag();
		foreach (var team in aliveTeams) aliveTeamsTag.Add(team);

		var winningTeams = new List<int>();
		foreach (var team in teams)
		{
			var context = new ScriptContext(state, registry.DataDriven);
			context.PushScope();
			try
			{
				context.DeclareVariable(WinBindings.Team, new IntTag(team));
				context.DeclareVariable(WinBindings.TeamAlive, new BoolTag(aliveTeams.Contains(team)));
				context.DeclareVariable(WinBindings.AliveTeamCount, new IntTag(aliveTeams.Count));
				context.DeclareVariable(WinBindings.AlivePlayerCount, new IntTag(alivePlayers.Count));
				context.DeclareVariable(WinBindings.AliveTeams, aliveTeamsTag);
				context.DeclareVariable(ScriptBindings.State, new ObjectRefTag(state));
				context.DeclareVariable(ScriptBindings.Vars, new ObjectRefTag(context));

				if (context.Evaluate(rules.WinCondition).IsTruthy) winningTeams.Add(team);
			}
			finally
			{
				context.PopScope();
			}
		}

		return winningTeams.Count switch
		{
			0 => new([], false),
			1 => new(winningTeams, false),
			_ => new(winningTeams, true)
		};
	}
}
