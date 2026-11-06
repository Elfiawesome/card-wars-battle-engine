using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Rules;

public sealed class HeroLoseConditionEvaluator(DataDrivenRegistry dataDriven)
{
	public bool IsDefeated(GameState state, GenericCard hero)
	{
		if (hero.Data.Get("lose_condition") is not CompoundTag conditionTag)
			return hero.Hrt <= 0;

		IExpression condition;
		try
		{
			condition = DataTagMapper.FromTag<IExpression>(conditionTag);
		}
		catch (Exception ex)
		{
			Log.Warn($"Could not parse lose_condition for hero [{hero.Id}]: {ex.Message}");
			return hero.Hrt <= 0;
		}

		var context = new ScriptContext(state, dataDriven);
		context.PushScope();
		try
		{
			context.DeclareVariable(ScriptBindings.Self, new ObjectRefTag(hero));
			context.DeclareVariable(ScriptBindings.Owner, new ObjectRefTag(hero));
			context.DeclareVariable(ScriptBindings.State, new ObjectRefTag(state));
			context.DeclareVariable(ScriptBindings.Vars, new ObjectRefTag(context));

			return context.Evaluate(condition).IsTruthy;
		}
		finally
		{
			context.PopScope();
		}
	}
}
