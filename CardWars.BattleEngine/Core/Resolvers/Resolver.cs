using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public abstract class Resolver
{
	public event Action<List<ActionBatch>>? OnCommited;
	public event Action<Resolver>? OnResolverQueued;
	public event Action? OnResolved;

	public enum ResolverState
	{
		Idle,
		Resolving,
		Resolved,
	}

	public ResolverState State = ResolverState.Idle;

	protected List<ActionBatch> ActionBatches = [];

	public abstract void Resolve(GameState state);

	protected void QueueResolver(Resolver resolver)
	{
		if (State == ResolverState.Resolved) { return; }
		OnResolverQueued?.Invoke(resolver);
	}

	protected List<ActionBatch> CallResolver(Resolver resolver, GameState state)
	{
		if (State == ResolverState.Resolved) { return []; }
		resolver.Resolve(state);
		resolver.Resolved(); // Force finish this resolver
		return resolver.ActionBatches;
	}


	protected void AddActionBatch(ActionBatch action)
	{
		ActionBatches.Add(action);
	}

	protected void CommitResolve()
	{
		Commit();
		Resolved();
	}

	protected void Commit()
	{
		if (State == ResolverState.Resolved) { return; }
		if (ActionBatches.Count == 0) { return; }
		OnCommited?.Invoke(ActionBatches);
		ActionBatches = [];
	}

	protected void Resolved()
	{
		if (State == ResolverState.Resolved) { return; }
		OnResolved?.Invoke();
		State = ResolverState.Resolved;
	}
}