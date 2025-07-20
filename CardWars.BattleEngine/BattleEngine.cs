using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.Actions.ActionDatas;
using CardWars.BattleEngine.Core.Events;
using CardWars.BattleEngine.Core.Resolvers;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	private GameState _gameState = new();
	private EventManager _eventManager = new();
	private List<Resolver> _resolverStack = new();

	public void _InitializeGame()
	{
		// TODO remove this temporary starting area
		QueueActionBatch(new([
			new CreateUnitSlotActionData(_gameState.UnitSlotIdCounter),
			new CreateUnitSlotActionData(_gameState.UnitSlotIdCounter),
			new CreateUnitSlotActionData(_gameState.UnitSlotIdCounter),
			new CreateUnitSlotActionData(_gameState.UnitSlotIdCounter)
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
			// Handle each actions individually here
			switch (action)
			{
				case CreateUnitActionData createUnitActionData:
					Console.WriteLine($"Creating UNIT[{createUnitActionData.UnitId}]");
					_gameState.Units.TryAdd(createUnitActionData.UnitId, new(createUnitActionData.UnitId));
					break;
				case CreateUnitSlotActionData createUnitSlotActionData:
					Console.WriteLine($"Creating UNITSLOT[{createUnitSlotActionData.UnitSlotId}]");
					_gameState.UnitSlots.TryAdd(createUnitSlotActionData.UnitSlotId, new(createUnitSlotActionData.UnitSlotId));
					break;

				case AttachUnitToUnitSlotActionData attachUnitToUnitSlotActionData:
					Console.WriteLine($"Attaching UNIT[{attachUnitToUnitSlotActionData.UnitId}] to UNITSLOT[{attachUnitToUnitSlotActionData.UnitSlotId}]");
					if (_gameState.UnitSlots.TryGetValue(attachUnitToUnitSlotActionData.UnitSlotId, out var unitSlot))
					{
						if (_gameState.Units.TryGetValue(attachUnitToUnitSlotActionData.UnitId, out var asdasd))
						{
							// unit;// TODO attach unit slot id to unit.ParentUnitSlotId
							unitSlot.HoldingUnit = attachUnitToUnitSlotActionData.UnitId;
						}
					}
					break;

				case UpdateUnitActionData updateUnitActionData:
					Console.WriteLine($"Updating UNIT[{updateUnitActionData.UnitId}]");
					if (_gameState.Units.TryGetValue(updateUnitActionData.UnitId, out var unit))
					{
						if (updateUnitActionData.Name != null) unit.Name = updateUnitActionData.Name;
						if (updateUnitActionData.Hp != null) unit.Hp = updateUnitActionData.Hp ?? 0;
						if (updateUnitActionData.Atk != null) unit.Atk = updateUnitActionData.Atk ?? 0;
						if (updateUnitActionData.Pt != null) unit.Pt = updateUnitActionData.Pt ?? 0;
					}
					break;
				default:
					break;
			}
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
			currentResolver.Resolve(_gameState, _eventManager);
		}
	}
}