using CardWars.BattleEngine.Core.Actions;
using CardWars.BattleEngine.Core.GameState;
using CardWars.BattleEngine.Core.Processes;

namespace CardWars.BattleEngine;

public class BattleEngine
{
	// Temperory API
	public GameState GameState => _gameState;

	// Internal APIs
	internal GameState _gameState = new();


	// Self usage
	private List<GameProcess> _processStack = [];

	public BattleEngine()
	{
		// Imitate Summoning Unit
		QueueProcess(new SummonUnitProcess());
	}

	public void QueueProcess(GameProcess process)
	{
		_processStack.Add(process);
		HandleProcessStack();
	}

	internal void QueueAction(GameActionBatch batch)
	{
		// For now we run the action immediately
		foreach (var action in batch.Actions)
		{
			action.Execute(this);
		}
	}

	private void HandleProcessStack()
	{
		if (_processStack.Count == 0)
			return;

		var currentProcess = _processStack[0];

		if (currentProcess.State == GameProcess.ProcessState.Unprocessed)
		{
			currentProcess.State = GameProcess.ProcessState.Processing;
			currentProcess.CompletedEvent += () =>
			{
				_processStack.RemoveAt(0);
				HandleProcessStack();
			};
			currentProcess.Execute(this);
		}
	}
}