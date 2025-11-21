namespace CardWars.BattleEngine.Event;

public interface IEventListener
{
	public int EventPriority { get; }

	void OnGameEvent(BattleEngine engine, IEvent gameEvent);
}