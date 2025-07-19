using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Resolvers;

public abstract class Resolver
{
	public event Action<List<ActionData>>? OnCommited;
	public event Action? OnResolved;

	public enum ResolverState
	{
		Idle,
		Resolving,
		Resolved,
	}

	public ResolverState State = ResolverState.Idle;

	protected List<ActionData> Actions = [];

	public abstract void Resolve(GameState state);

	protected void AddActions(ActionData action)
	{
		Actions.Add(action);
	}

	protected void CommitResolve()
	{
		Commit();
		Resolved();
	}

	protected void Commit()
	{
		if (State == ResolverState.Resolved) { return; }
		if (Actions.Count == 0) { return; }
		OnCommited?.Invoke(Actions);
		Actions = [];
	}

	protected void Resolved()
	{
		if (State == ResolverState.Resolved) { return; }
		OnResolved?.Invoke();
		State = ResolverState.Resolved;
	}
}