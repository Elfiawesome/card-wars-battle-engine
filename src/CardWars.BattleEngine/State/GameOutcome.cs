using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.State;

public enum GameOutcomeKind
{
	None,
	Win,
	Draw
}

[DataTagType("battle/outcome")]
public sealed record GameOutcome(
	[property: DataTag] GameOutcomeKind Kind,
	[property: DataTag] int? WinningTeam
)
{
	public static GameOutcome Continue { get; } = new(GameOutcomeKind.None, null);

	public bool IsGameOver => Kind != GameOutcomeKind.None;
	public bool IsDraw => Kind == GameOutcomeKind.Draw;

	public static GameOutcome Win(int winningTeam) => new(GameOutcomeKind.Win, winningTeam);
	public static GameOutcome Draw() => new(GameOutcomeKind.Draw, null);
}
