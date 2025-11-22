namespace CardWars.BattleEngine.Event;

public interface IEventListener
{
	public int EventPriority { get; }

	public void OnGameEvent(BattleEngine engine, IEvent gameEvent, EventResponse eventResponse);
}