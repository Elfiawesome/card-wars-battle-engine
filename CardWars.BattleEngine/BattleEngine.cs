using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private GameState _gameState = new();
	private List<Resolver> resolverStack = new();

	public void QueueResolver(Resolver resolver)
	{
		resolverStack.Add(resolver);
		HandleResolvers();
	}

	private void QueueActionBatch(ActionBatch actionBatch)
	{
		actionBatch.Actions.ForEach(action =>
		{
			// Handle each actions individually here
			switch (action)
			{
				default:
					break;
			}
		});
	}

	private void HandleResolvers()
	{
		if (resolverStack.Count == 0) { return; }
		Resolver currentResolver = resolverStack[0];
		if (currentResolver.State == Resolver.ResolverState.Idle)
		{
			currentResolver.State = Resolver.ResolverState.Resolving;
			currentResolver.OnResolved += () => { resolverStack.RemoveAt(0); HandleResolvers(); };
			currentResolver.OnCommited += (actionBatches) => { actionBatches.ForEach(action => QueueActionBatch(action)); };
			currentResolver.Resolve(_gameState);
		}
	}
}