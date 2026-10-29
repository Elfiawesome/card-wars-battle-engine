using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Targeting;

public sealed class CardTargetFilterEvaluator(DataDrivenRegistry dataDriven)
{
	public bool CanTarget(GameState state, GenericCard card, EntityId playerId, EntityId? targetId)
	{
		var filter = GetFilter(card);
		if (filter == null) return targetId == null;
		if (targetId == null) return false;

		var target = state.Get(targetId.Value);
		if (target == null) return false;

		var context = CreateContext(state, card, playerId);
		try
		{
			BindCandidate(context, target);
			return context.Evaluate(filter).IsTruthy;
		}
		finally
		{
			context.PopScope();
		}
	}

	public IReadOnlyCollection<EntityId> ResolveTargets(GameState state, GenericCard card, EntityId playerId)
	{
		var filter = GetFilter(card);
		if (filter == null) return [];

		var targets = new List<EntityId>();
		var context = CreateContext(state, card, playerId);
		try
		{
			foreach (var entity in state.All)
			{
				BindCandidate(context, entity);
				if (context.Evaluate(filter).IsTruthy) targets.Add(entity.Id);
			}
		}
		finally
		{
			context.PopScope();
		}

		return targets;
	}

	private ScriptContext CreateContext(GameState state, GenericCard card, EntityId playerId)
	{
		var context = new ScriptContext(state, dataDriven);
		context.PushScope();
		context.DeclareVariable(TargetBindings.Card, new ObjectRefTag(card));
		context.DeclareVariable(TargetBindings.Player, state.Get(playerId) is { } player ? new ObjectRefTag(player) : new NullTag());

		DataTag ownerTag = card.OwnerPlayerId is { } ownerId && state.Get(ownerId) is { } owner
			? new ObjectRefTag(owner)
			: new NullTag();
		context.DeclareVariable(ScriptBindings.Owner, ownerTag);
		context.DeclareVariable(ScriptBindings.Self, ownerTag);
		context.DeclareVariable(ScriptBindings.State, new ObjectRefTag(state));
		context.DeclareVariable(ScriptBindings.Vars, new ObjectRefTag(context));
		return context;
	}

	private static void BindCandidate(ScriptContext context, IEntity candidate)
		=> context.DeclareVariable(TargetBindings.Candidate, new ObjectRefTag(candidate));

	private static IExpression? GetFilter(GenericCard card)
	{
		var tag = card.Data.Get("target_filter");
		if (tag == null) return null;

		if (tag is not CompoundTag compound)
		{
			Log.Warn($"Card [{card.Id}] target_filter must be a single expression");
			return null;
		}

		try
		{
			return DataTagMapper.FromTag<IExpression>(compound);
		}
		catch (Exception ex)
		{
			Log.Error($"Could not parse target_filter for card [{card.Id}]: {ex.Message}");
			return null;
		}
	}
}
