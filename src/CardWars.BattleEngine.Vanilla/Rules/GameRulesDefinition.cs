using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Rules;

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

	public static GameRulesDefinition? Get(BattleEngineRegistry registry, ResourceId ruleId)
	{
		if (registry.RuleDefinitions.Get(ruleId) is not CompoundTag tag) return null;

		try
		{
			return DataTagMapper.FromTag<GameRulesDefinition>(tag);
		}
		catch (Exception ex)
		{
			Log.Warn($"Could not parse game rules [{ruleId}]: {ex.Message}");
			return null;
		}
	}
}
