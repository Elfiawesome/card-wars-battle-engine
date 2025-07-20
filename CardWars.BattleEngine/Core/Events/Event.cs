namespace CardWars.BattleEngine.Core.Events;

public class EventLink<TEventContext, TEventSubscriber> where TEventContext : EventContext where TEventSubscriber : EventSubscriber<TEventContext>
{
	public List<TEventSubscriber> Subscribers = [];

	public void Subscribe(TEventSubscriber subscriber)
	{
		if (Subscribers.Contains(subscriber)) { return; }
		Subscribers.Add(subscriber);
	}

	public void Unsubscribe(TEventSubscriber subscriber)
	{
		if (!Subscribers.Contains(subscriber)) { return; }
		Subscribers.Remove(subscriber);
	}

	public void Raise(TEventContext context)
	{
		Subscribers.ForEach((subscriber) =>
		{
			subscriber.Handle(context);
		});
	}
}

public abstract class EventSubscriber<TEventContext> where TEventContext : EventContext
{
	public abstract void Handle(TEventContext context);
}

public abstract class EventContext
{

}