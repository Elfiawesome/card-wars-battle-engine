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

	public bool IsIdle => _behaviourQueue.Count == 0 && _eventQueue.Count == 0 && _activeEvent == null;

	private readonly Queue<IEvent> _eventQueue = [];
	private readonly List<BehaviourExecution> _behaviourQueue = [];
	private IEvent? _activeEvent = null;

	// Public APIs entries
	public void ProcessInput(EntityId playerId, IInput input)
	{
		// Input handled to current behaviour
		bool inputConsumed = ResumeBehaviours(playerId, input);

		// Handle input if no behaviours used the input
		if (!inputConsumed)
		{
			if ((playerId == Guid.Empty) || State.Turn.AllowedPlayerInputs.Contains(playerId))
			{
				Registry.InputHandlers.Execute(new InputContext(this, playerId), input);
			}
		}

		// Process until end
		Process();
	}

	public void ApplyBlockBatch(BlockBatch batch)
	{
		foreach (var block in batch.Blocks ?? [])
		{
			Registry.BlockHandlers.Execute(State, block);
			Console.WriteLine($"Executing Block: {Helper.SerializeBlock(block)}");
		}
	}

	public void QueueEvent(IEvent evnt) => _eventQueue.Enqueue(evnt);

	// Private functions
	private void Process()
	{
		int count = 0;
		while (count++ <= ProcessStackLimit)
		{
			// Handle pending behaviours first & exits if any one of them is waiting for input 
			// (meaning the event does not get handled)
			if (_behaviourQueue.Count > 0)
			{
				var exec = _behaviourQueue[0];

				if (exec.IsWaiting) return; // Still waiting for input and wait

				var result = exec.Behaviour.Start(exec.Event, exec.Context);

				if (result == BehaviourResult.WaitForInput)
				{
					exec.IsWaiting = true;
					return; // Exit and wait
				}

				if (result == BehaviourResult.Complete)
				{
					exec.Context.CommitStagedBlocks();
					_behaviourQueue.RemoveAt(0); 
				}
				continue;
			}

			// Handle event after all behaviours are handled already
			if (_activeEvent != null)
			{
				Registry.EventHandlers.Execute(this, _activeEvent);
				_activeEvent = null;
				continue;
			}

			// Handle events from queue
			if (_eventQueue.Count > 0)
			{
				_activeEvent = _eventQueue.Dequeue();
				QueueBehavioursForEvent(_activeEvent);
				continue;
			}

			// If everything is empty, there is no action needed and we end here
			break;
		}

		// Cleanup if infinite loop
		if (count > ProcessStackLimit)
		{
			_eventQueue.Clear();
			_behaviourQueue.Clear();
			_activeEvent = null;
		}
	}

	private void QueueBehavioursForEvent(IEvent evnt)
	{
		var eventType = evnt.GetType();
		var pointers = State.GetAllBehaviourPointers();

		var executions = new List<(
			int entityPriority,
			IBehaviour behaviour,
			BehaviourContext behaviourContext)>();

		foreach (var (entityId, pointer) in pointers)
		{
			var behaviour = InstantiateBehaviour(pointer, eventType);
			if (behaviour == null) continue;

			var entity = State.Get(entityId);
			var behaviourContext = new BehaviourContext(this, State, entityId);
			executions.Add((entity?.BehaviourPriority ?? 0, behaviour, behaviourContext));
		}

		// Sort by entity priority, then behaviour priority, and queue them
		foreach (var (_, behaviour, context) in executions.OrderBy(e => e.entityPriority).ThenBy(e => e.behaviour.Priority))
		{
			_behaviourQueue.Add(new BehaviourExecution(behaviour, context, evnt));
		}
	}

	private bool ResumeBehaviours(EntityId playerId, IInput input)
	{
		if (_behaviourQueue.Count == 0) return false;

		var exec = _behaviourQueue[0];
		if (!exec.IsWaiting) return false; // Usually not the case, but just in case

		var result = exec.Behaviour.Resume(playerId, input, exec.Context);

		if (result == BehaviourResult.WaitForInput)
		{
			return true;
		}

		if (result == BehaviourResult.Complete)
		{
			exec.Context.CommitStagedBlocks();
			_behaviourQueue.RemoveAt(0);
		}
		return true;
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
		}

		if (behaviour?.ListeningEventType != eventType) return null;

		return behaviour;
	}

	private class BehaviourExecution(IBehaviour behaviour, BehaviourContext context, IEvent evnt)
	{
		public IBehaviour Behaviour { get; } = behaviour;
		public BehaviourContext Context { get; } = context;
		public IEvent Event { get; } = evnt;
		public bool IsWaiting { get; set; } = false;
	}
}