namespace CardWars.BattleEngine.Event;

public interface IEventSubscriber<TEvent>
	where TEvent : IEvent
{
	public int Priority => 0;
	public void Handle(TEvent evnt);
}