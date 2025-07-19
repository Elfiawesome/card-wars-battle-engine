using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private GameState _gameState = new();
	private List<GameResolver> resolverStack = new();

	public void QueueResolver(GameResolver resolver)
	{
		resolverStack.Add(resolver);
		HandleResolvers();
	}

	private void QueueAction(GameAction action)
	{
		action.Execute(_gameState);
	}

	private void HandleResolvers()
	{
		if (resolverStack.Count == 0) { return; }
		GameResolver currentResolver = resolverStack[0];
		if (currentResolver.State == GameResolver.ResolverState.Idle)
		{
			currentResolver.State = GameResolver.ResolverState.Resolving;
			currentResolver.OnResolved += () => { resolverStack.RemoveAt(0); HandleResolvers(); };
			currentResolver.OnCommited += (actions) => { actions.ForEach(action => QueueAction(action)); };
			currentResolver.Resolve(_gameState);
		}
	}
}