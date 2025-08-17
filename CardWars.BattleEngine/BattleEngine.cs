using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private GameState _gameState = new();
	private ActionHandlerManager _actionHandlerManager = new();
	private List<Resolver> _resolverStack = new();


	public void _InitializeGame()
	{
		// TODO remove this temporary starting area
		QueueActionBatch(new([
		]));
	}

	public void QueueResolver(Resolver resolver)
	{
		_resolverStack.Add(resolver);
		HandleResolvers();
	}

	private void QueueActionBatch(ActionBatch actionBatch)
	{
		actionBatch.Actions.ForEach(action =>
		{
			_actionHandlerManager.HandleActionData(_gameState, action);
		});
	}

	private void HandleResolvers()
	{
		if (_resolverStack.Count == 0) { return; }

		var currentResolver = _resolverStack[0];
		if (currentResolver.State == Resolver.ResolverState.Idle)
		{
			currentResolver.State = Resolver.ResolverState.Resolving;
			currentResolver.OnResolved += () => { _resolverStack.RemoveAt(0); HandleResolvers(); };
			currentResolver.OnCommited += (actionBatches) => { actionBatches.ForEach(action => QueueActionBatch(action)); };
			currentResolver.OnResolverQueued += _resolverStack.Add;
			currentResolver.Resolve(_gameState);
		}
	}
}