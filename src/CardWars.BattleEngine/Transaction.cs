using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class Transaction
{
	public const int ProcessStackLimit = 1000;
	public required BattleEngineRegistry Registry { get; init; }
	public required GameState State { get; init; }
	public bool IsIdle => _behaviourPending.Count == 0 && _eventQueue.Count == 0;

	private Queue<IEvent> _eventQueue = [];
	private List<(IBehaviour, BehaviourContext)> _behaviourPending = [];

	// Public APIs entries
	public void ProcessInput(EntityId playerId, IInput input)
	{
		// Resume any leftover behaviours
		ResumeBehaviours(playerId, input);
		// Handle main input via handler
		if ((playerId == Guid.Empty) || State.Turn.AllowedPlayerInputs.Contains(playerId))
		{
			Registry.InputHandlers.Execute(new InputContext(this, playerId), input);
		}
		// Process & resolve until end
		Process();
	}

	public void ApplyBlockBatch(BlockBatch batch)
	{
		foreach (var block in batch.Blocks ?? [])
		{
			Registry.BlockHandlers.Execute(State, block);
		}
	}

	public void QueueEvent(IEvent evnt)
	{
		_eventQueue.Enqueue(evnt);
	}

	// Private functions
	private void Process()
	{
		int count = 0;
		while (_eventQueue.Count > 0)
		{
			var evnt = _eventQueue.Dequeue();

			// Handle for behaviours
			HandleBehaviourEvents(evnt);

			// Finally handle event
			Registry.EventHandlers.Execute(this, evnt);
			if (count++ > ProcessStackLimit) { break; }
		}
	}

	private void HandleBehaviourEvents(IEvent evnt)
	{
		var eventType = evnt.GetType();
		var pointers = State.GetAllBehaviourPointers();

		var behaviourContexts = new List<(
			int entityPriority,
			IBehaviour behaviour,
			BehaviourContext behaviourContext)>();

		foreach (var (entityId, pointer) in pointers)
		{
			var behaviour = InstantiateBehaviour(pointer, eventType);
			if (behaviour == null) { continue; }

			var entity = State.Get(entityId);
			var behaviourContext = new BehaviourContext(this, State, entityId);
			behaviourContexts.Add((entity?.BehaviourPriority ?? 0, behaviour, behaviourContext));
		}

		foreach ((int priority, IBehaviour behaviour, BehaviourContext behaviourContext) in behaviourContexts.OrderBy(e => e.entityPriority).ThenBy(e => e.behaviour.Priority))
		{
			var result = behaviour.Start(evnt, behaviourContext);
			if (result == BehaviourResult.WaitForInput) { _behaviourPending.Add((behaviour, behaviourContext)); }
			if (result == BehaviourResult.Complete) { behaviourContext.CommitStagedBlocks(); }
		}
	}

	private void ResumeBehaviours(EntityId playerId, IInput input)
	{
		for (int i = _behaviourPending.Count - 1; i >= 0; i--)
		{
			var (behaviour, behaviourContext) = _behaviourPending[i];
			var result = behaviour.Resume(playerId, input, behaviourContext);
			if (result == BehaviourResult.WaitForInput) { continue; }
			if (result == BehaviourResult.Complete)
			{
				_behaviourPending.RemoveAt(i);
				behaviourContext.CommitStagedBlocks();
			}
		}
	}

	private IBehaviour? InstantiateBehaviour(BehaviourPointer pointer, Type eventType)
	{
		IBehaviour? behaviour = null;
		if (pointer.BehaviourResource is { } resourceId)
		{
			behaviour = Registry.Behaviours.Create(resourceId);
		}
		else if (pointer.BehaviourDefinition != null)
		{
			// TODO: Data-driven behaviour creation
			// behaviour = CreateDataDrivenBehaviour(pointer.BehaviourDefinition);
		}

		if (behaviour?.ListeningEventType != eventType) { return null; }

		return behaviour;
	}
}