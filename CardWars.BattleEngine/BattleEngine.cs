using CardWars.BattleEngine.Core;
using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.Actions.ActionHandlers;
using CardWars.BattleEngine.Core.Inputs;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;
using CardWars.BattleEngine.Definitions;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private GameState _gameState = new();
	public GameState GameState => _gameState; // TODO: To remove later
	public DefinitionLibrary Definitions = new();
	private ActionHandlerManager _actionHandlerManager = new();
	private List<Resolver> _resolverStack = new();

	public BattleEngine()
	{
	}

	public void HandleInput(IInputData input)
	{
		// Do my own input handling

		// pass input to resolver
		if (_resolverStack.Count == 0) { return; }
		var currentResolver = _resolverStack[0];
		if (currentResolver.State != Resolver.ResolverState.Resolved)
		{
			currentResolver.OnPlayerInput(input);
		}
	}

	internal void QueueResolver(Resolver resolver)
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
			Console.WriteLine($"Runnin Resolver [{currentResolver.GetType().Name}]");
			currentResolver.State = Resolver.ResolverState.Resolving;
			currentResolver.OnResolved += () => { _resolverStack.RemoveAt(0); HandleResolvers(); };
			currentResolver.OnCommited += (actionBatches) => { actionBatches.ForEach(action => QueueActionBatch(action)); };
			currentResolver.OnResolverQueued += _resolverStack.Add;
			currentResolver.Resolve(_gameState);
		}
	}

	public PlayerId NewPlayerJoined()
	{
		PlayerId pid = new(_gameState.NewId);
		BattlefieldId bid = new(_gameState.NewId);
		QueueActionBatch(new([
			new InstantiatePlayerData(pid),
			new InstantiateBattlefieldData(bid),
			new AttachBattlefieldToPlayerData(bid, pid),
		]));

		// Create a battlefield
		QueueResolver(new CreateBattlefieldResolver(bid, pid));

		// Assumes the battlefield has already created the unit slots
		UnitSlotId usid = _gameState.Battlefields[bid].UnitSlots[0];
		QueueResolver(new SummonUnitResolver(usid, ""));
		usid = _gameState.Battlefields[bid].UnitSlots[1];
		QueueResolver(new SummonUnitResolver(usid, ""));
		usid = _gameState.Battlefields[bid].UnitSlots[2];
		QueueResolver(new SummonUnitResolver(usid, ""));
		return pid;
	}
}