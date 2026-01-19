using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	public BattleEngineRegistry Registry = new();
	public GameState State = new();
	private Transaction? _currentTransaction;

	public void LoadMod(IBattleEngineMod mod)
	{
		mod.OnLoad(Registry);
	}

	public void HandleInput(IInput input, List<IBehaviour>? DEBUG_testBehaviour = null)
	{
		if (_currentTransaction == null)
		{
			_currentTransaction = new Transaction() { Registry = Registry, State = State, DEBUG_testBehaviour = DEBUG_testBehaviour ?? [] };
		}

		_currentTransaction.ProcessInput(input);
		if (_currentTransaction.IsResolved) _currentTransaction = null;
	}
}

public class Transaction
{
	public required BattleEngineRegistry Registry;
	public required GameState State;
	public Queue<IEvent> PendingEvents = [];

	public bool IsResolved => PendingEvents.Count == 0;

	public List<IBehaviour> DEBUG_testBehaviour = [];

	public void ProcessInput(IInput input)
	{
		Registry.InputHandlers.Execute(this, input);

		int recursionCount = 0;
		while (true)
		{
			bool newWork = false;

			if (PendingEvents.Count > 0)
			{
				var evnt = PendingEvents.Dequeue();

				// Get all listeners with behaviours (from state and retrieve the BehaviourPointer)
				foreach (var behvaiour in DEBUG_testBehaviour)
				{
					if (behvaiour.ListeningEventType == evnt.GetType()) { behvaiour.OnEvent(evnt); }
				}

				Registry.EventHandlers.Execute(this, evnt);
				newWork = true;
			}

			if (recursionCount > 100) { break; }
			if (newWork == false) { break; }
			recursionCount++;
		}
	}

	public void RaiseEvent(IEvent evnt) => PendingEvents.Enqueue(evnt);
}