namespace CardWars.BattleEngine.Core.Processes;

public abstract class GameProcess
{
	public enum ProcessState
	{
		Unprocessed,
		Processing,
		Completed
	}

	public ProcessState State = ProcessState.Unprocessed;
	public event Action? CompletedEvent;

	// Executes a process using BattleEngine as a REFERENCE (not to edit it through here. It is a 'readonly' here)
	public abstract void Execute(BattleEngine engine);

	protected void Complete()
	{
		if (State != ProcessState.Completed)
		{
			State = ProcessState.Completed;
			CompletedEvent?.Invoke();
		}
	}
}


// Need Better names for the children!
// This one is used more like as a a cascading effect
public abstract class QueueGameProcess : GameProcess
{
}

// This one is used more like as a 'blueprint'
public abstract class InstantGameProcess : GameProcess
{
}