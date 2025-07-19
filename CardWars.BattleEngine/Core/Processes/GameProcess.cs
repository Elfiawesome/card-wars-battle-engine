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


// Need Better names for the children
public abstract class QueueGameProcess : GameProcess
{
	
}

public abstract class InstantGameProcess : GameProcess
{

}