using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.Actions.ActionHandlers;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private GameState _gameState = new();
	public GameState GameState => _gameState;
	private ActionHandlerManager _actionHandlerManager = new();
	private List<Resolver> _resolverStack = new();

	public BattleEngine()
	{
		// New Player Enters
		for (int playeri = 0; playeri < 2; playeri++)
		{
			PlayerId pid = new(_gameState.NewId);
			BattlefieldId bid = new(_gameState.NewId);
			QueueActionBatch(new([
				new InstantiatePlayerData(pid),
				new InstantiateBattlefieldData(bid),
			]));
			UnitSlotId? unitSlotId = null;
			for (int i = 0; i < 3; i++)
			{
				UnitSlotId usid = new(_gameState.NewId);
				if (unitSlotId == null) { unitSlotId = usid; }
				QueueActionBatch(new([
					new InstantiateUnitSlotData(usid),
					new AttachUnitSlotToBattlefieldData(usid, bid),
				]));
			}
			// Send Resolver to summon a unit
			QueueResolver(new SummonUnitResolver(unitSlotId ?? new(0), "Unit Definition Id"));
		}

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
			Console.WriteLine($"Runnin Resolver [{currentResolver.GetType().Name}]");
			currentResolver.State = Resolver.ResolverState.Resolving;
			currentResolver.OnResolved += () => { _resolverStack.RemoveAt(0); HandleResolvers(); };
			currentResolver.OnCommited += (actionBatches) => { actionBatches.ForEach(action => QueueActionBatch(action)); };
			currentResolver.OnResolverQueued += _resolverStack.Add;
			currentResolver.Resolve(_gameState);
		}
	}
}