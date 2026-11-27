using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Rules;

[DataTagType("battle/rules")]
public sealed class GameRulesDefinition
{
	public static readonly ResourceId DefaultId = ResourceId.Vanilla("rules/default");

	[DataTag] public List<int> PtSchedule { get; set; } = [];
	[DataTag] public int PtCap { get; set; }
	[DataTag] public IExpression? WinCondition { get; set; }
	[DataTag] public List<UnitSlotPos> UnitSlots { get; set; } = [];
	[DataTag] public ResourceId IntroductionAnimation { get; set; } = ResourceId.Empty;
	[DataTag] public IExpression? UnitTargetFilter { get; set; }

	public int GetPtForRound(int roundNumber)
	{
		var index = Math.Clamp(roundNumber - 1, 0, PtSchedule.Count - 1);
		return Math.Min(PtSchedule[index], PtCap);
	}

	public static GameRulesDefinition? Get(BattleEngineRegistry registry, ResourceId ruleId)
	{
		if (registry.GetExtension<CardWarsDefinitions>()?.RuleDefinitions.Get(ruleId) is not CompoundTag tag) return null;

		try
		{
			return registry.DataTags.FromTag<GameRulesDefinition>(tag);
		}
		catch (Exception ex)
		{
			registry.Logger.Warn($"Could not parse game rules [{ruleId}]: {ex.Message}");
			return null;
		}
	}
}
