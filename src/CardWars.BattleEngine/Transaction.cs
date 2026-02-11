using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public enum TransactionState { Idle, Processing, WaitingForInput }

public class Transaction
{
	public required BattleEngineRegistry Registry { get; init; }
	public required GameState State { get; init; }

	private TransactionState _state = TransactionState.Idle;
	private readonly Queue<IEvent> _eventQueue = new();
	private readonly Queue<BehaviourExecution> _behaviourQueue = new();
	private BehaviourExecution? _activeBehaviour;

	public bool IsIdle => _state == TransactionState.Idle;
	public bool IsWaitingForInput => _state == TransactionState.WaitingForInput;
	public EntityId? InputPlayerId { get; set; } = null;
	public void ProcessInput(EntityId playerId, IInput input)
	{
		if (_state == TransactionState.WaitingForInput && _activeBehaviour != null)
		{
			ResumeActiveBehaviour(input);
		}
		else
		{
			if ((playerId == Guid.Empty) || State.Turn.AllowedPlayerInputs.Contains(playerId))
			{
				InputPlayerId = playerId; // Im too lazy to inject the player id into the handler .-.
				Registry.InputHandlers.Execute(this, input);
				InputPlayerId = null;
			}
		}

		ProcessUntilBlocked();
	}

	public void QueueEvent(IEvent evnt) => _eventQueue.Enqueue(evnt);

	public void ApplyBlockBatch(BlockBatch batch)
	{
		// TODO: Move this outta here and make battle engine handle this and event it out
		foreach (var block in batch.Blocks ?? [])
		{
			Registry.BlockHandlers.Execute(State, block);
		}
	}


	private void ProcessUntilBlocked()
	{
		_state = TransactionState.Processing;
		int safety = 0;

		while (safety++ < 1000)
		{
			if (_behaviourQueue.Count > 0)
			{
				var execution = _behaviourQueue.Dequeue();
				var result = execution.Behaviour.Start(execution.Event, execution.Context);

				if (HandleBehaviourResult(execution, result))
					return; // Wait for input

				continue;
			}

			if (_eventQueue.Count > 0)
			{
				var evnt = _eventQueue.Dequeue();
				QueueBehavioursForEvent(evnt);
				continue;
			}

			break;
		}

		_state = TransactionState.Idle;
	}

	private void QueueBehavioursForEvent(IEvent evnt)
	{
		var eventType = evnt.GetType();
		var pointers = State.GetAllBehaviourPointers();

		var executions = new List<(int entityPriority, int behaviourPriority, BehaviourExecution exec)>();

		foreach (var (entityId, pointer) in pointers)
		{
			var behaviour = InstantiateBehaviour(pointer, eventType);
			if (behaviour == null) continue;

			var entity = State.Get(entityId);
			var context = new BehaviourContext(this, State, entityId);

			executions.Add((
				entity?.BehaviourPriority ?? 0,
				behaviour.Priority,
				new BehaviourExecution(behaviour, context, evnt)
			));
		}

		// Sort by entity priority, then behaviour priority
		foreach (var exec in executions.OrderBy(e => e.entityPriority).ThenBy(e => e.behaviourPriority))
		{
			_behaviourQueue.Enqueue(exec.exec);
		}

		// Run the handler
		Registry.EventHandlers.Execute(this, evnt);
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

		if (behaviour?.ListeningEventType != eventType)
			return null;

		return behaviour;
	}

	private bool HandleBehaviourResult(BehaviourExecution execution, BehaviourResult result)
	{
		switch (result)
		{
			case BehaviourResult.WaitForInput:
				_activeBehaviour = execution;
				_state = TransactionState.WaitingForInput;
				return true;

			case BehaviourResult.Complete: // One shot behaviours
				execution.Context.CommitStagedBlocks();
				return false;

			case BehaviourResult.Continue: // Others...
			default:
				// Re-queue for continued processing (or handle internally)
				_behaviourQueue.Enqueue(execution);
				return false;
		}
	}

	private void ResumeActiveBehaviour(IInput input)
	{
		if (_activeBehaviour == null) return;

		var result = _activeBehaviour.Behaviour.Resume(input, _activeBehaviour.Context);

		if (!HandleBehaviourResult(_activeBehaviour, result))
		{
			_activeBehaviour = null;
		}
	}
}

// Internal record to track behaviour execution
internal record BehaviourExecution(
	IBehaviour Behaviour,
	BehaviourContext Context,
	IEvent Event
);