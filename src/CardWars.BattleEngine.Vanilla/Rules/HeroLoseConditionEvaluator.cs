using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Rules;

public sealed class HeroLoseConditionEvaluator(BattleEngineRegistry registry)
{
	public bool IsDefeated(GameState state, GenericCard hero)
	{
		var pointer = GetPointer(hero);

		if (pointer.Resource is { } resourceId)
		{
			var condition = registry.EntityConditions.Create(resourceId);
			if (condition is null)
			{
				Log.Warn($"Unknown hero lose condition [{resourceId}] for hero [{hero.Id}]");
				return UseDefault(state, hero);
			}

			return condition.IsMet(state, hero.Id);
		}

		if (pointer.Definition is not { } definition)
			return UseDefault(state, hero);

		IExpression conditionExpression;
		try
		{
			conditionExpression = DataTagMapper.FromTag<IExpression>(definition);
		}
		catch (Exception ex)
		{
			Log.Warn($"Could not parse lose_condition for hero [{hero.Id}]: {ex.Message}");
			return UseDefault(state, hero);
		}

		var context = new ScriptContext(state, registry.DataDriven);
		context.PushScope();
		try
		{
			context.DeclareVariable(ScriptBindings.Self, new ObjectRefTag(hero));
			context.DeclareVariable(ScriptBindings.Owner, new ObjectRefTag(hero));
			context.DeclareVariable(ScriptBindings.State, new ObjectRefTag(state));
			context.DeclareVariable(ScriptBindings.Vars, new ObjectRefTag(context));

			return context.Evaluate(conditionExpression).IsTruthy;
		}
		finally
		{
			context.PopScope();
		}
	}

	private bool UseDefault(GameState state, GenericCard hero)
	{
		var condition = registry.EntityConditions.Create(HeroHrtLoseCondition.Id);
		return condition?.IsMet(state, hero.Id) ?? hero.Hrt <= 0;
	}

	private static ContentPointer GetPointer(GenericCard hero)
	{
		if (hero.Data.Get("lose_condition") is not CompoundTag pointerTag)
			return new ContentPointer(Resource: HeroHrtLoseCondition.Id);

		try
		{
			return DataTagMapper.FromTag<ContentPointer>(pointerTag);
		}
		catch (Exception ex)
		{
			Log.Warn($"Could not parse lose_condition for hero [{hero.Id}]: {ex.Message}");
			return new ContentPointer(Resource: HeroHrtLoseCondition.Id);
		}
	}
}
