using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Rules;

[DataTagType("battle/rules")]
public sealed class GameRulesDefinition
{
	private const int FallbackPt = 5;

	public static readonly ResourceId DefaultId = ResourceId.Vanilla("rules/default");

	[DataTag] public List<int> PtSchedule { get; set; } = [];
	[DataTag] public int PtCap { get; set; } = 50;
	[DataTag] public IExpression? WinCondition { get; set; }

	public int GetPtForRound(int roundNumber)
	{
		if (PtSchedule.Count == 0) return Math.Min(FallbackPt, PtCap);

		var index = Math.Clamp(roundNumber - 1, 0, PtSchedule.Count - 1);
		return Math.Min(PtSchedule[index], PtCap);
	}
}
