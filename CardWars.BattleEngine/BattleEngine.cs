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

	public void HandleInput(PlayerId playerId, IInputData input)
	{
		if (!_gameState.CanPlayerPushInput(playerId)) { return; }
		// Do my own input handling (Need to abstract this somehow?)
		switch (input)
		{
			case EndTurnInputData endTurnInput:
				if (_gameState.CurrentPlayer != playerId) { return; }
				QueueResolver(new EndTurnResolver());
				break;
			case PlayerJoinedInputData playerJoinedInputData:
				break;
			case DrawCardFromDeckInputData drawCardFromDeckInputData:
				if (_gameState.Players.TryGetValue(playerId, out var player))
				{
					if (_gameState.Decks.TryGetValue(drawCardFromDeckInputData.DeckId, out var deck))
					{
						if (player.Decks.Contains(drawCardFromDeckInputData.DeckId))
						{
							// Console.WriteLine($"drawing from {deck.Id}");
						}
					}
				}
				break;
		}

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
		actionBatch.Actions.ForEach(actionContainer =>
		{
			// if actionContainer is for us then execute
			_actionHandlerManager.HandleActionData(_gameState, actionContainer.Action);
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
			currentResolver.OnCommited += (actionBatches) => { actionBatches.ForEach(actionBatch => QueueActionBatch(actionBatch)); };
			currentResolver.OnResolverQueued += _resolverStack.Add;
			currentResolver.Resolve(_gameState);
		}
	}

	public PlayerId NewPlayerJoined()
	{
		PlayerId pid = new(_gameState.NewId);
		BattlefieldId bid = new(_gameState.NewId);
		DeckId did = new(_gameState.NewId);
		QueueActionBatch(new([
			new InstantiatePlayerData(pid),
			new AddPlayerToTurnOrderData(pid),

			new InstantiateBattlefieldData(bid),
			new AttachBattlefieldToPlayerData(bid, pid),

			new IntstantiateDeckData(did, [
				"warrior",
			]),
			new AttachDeckToPlayerData(did, pid)
		]));

		return pid;
	}

	public void StartGame()
	{
		QueueActionBatch(new(
			new AdvanceTurnOrderData(0)
		));
	}
}