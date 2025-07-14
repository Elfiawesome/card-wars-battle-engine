namespace Elfiawesome.CardWarsBattleEngine.Game.Intents;

public abstract class GameIntent
{
	public bool IsCompleted { get; private set; }
	public Action? IntentCompletedEvent;

	public abstract void Execute(CardWarsBattleEngine engine);

	protected void OnIntentCompleted()
	{
		if (!IsCompleted)
		{
			IsCompleted = true;
			IntentCompletedEvent?.Invoke();
		}
	}
}