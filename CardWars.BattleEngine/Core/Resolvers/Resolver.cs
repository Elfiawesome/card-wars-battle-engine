using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;


// What Is The Purpose of a RESOLVER?
// A resolver is meant to sequentially handle snippets of process logic by creating action batches
public abstract class Resolver
{
	// Events
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