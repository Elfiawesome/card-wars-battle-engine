namespace CardWars.BattleEngine.Event;

public sealed class EventService
{
	private readonly Dictionary<Type, List<ISubscriberWrapper>> _subscribers = new();

	public void Subscribe<TEvent>(IEventSubscriber<TEvent> subscriber)
		where TEvent : IEvent
	{
		var eventType = typeof(TEvent);
		var wrapper = new SubscriberWrapper<TEvent>(subscriber);
		if (!_subscribers.TryGetValue(eventType, out var subscriberList))
		{
			subscriberList = [];
			_subscribers[eventType] = subscriberList;
		}

		subscriberList.Add(wrapper);
		subscriberList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
	}

	public void Unsubscribe<TEvent>(IEventSubscriber<TEvent> subscriber) where TEvent : IEvent
	{
		var eventType = typeof(TEvent);
		var subscriberList = _subscribers[eventType];
		subscriberList?.RemoveAll((s) => s is SubscriberWrapper<TEvent>);
	}

	public void Raise<TEvent>(TEvent evnt) where TEvent : IEvent
	{
		var eventType = typeof(TEvent);
		foreach (var subscriber in _subscribers[eventType])
		{
			subscriber.Handle(evnt);
		}
	}

	private interface ISubscriberWrapper
	{
		int Priority { get; }
		void Handle(IEvent evnt);
	}

	private class SubscriberWrapper<TEvent>(IEventSubscriber<TEvent> subscriber) : ISubscriberWrapper
		where TEvent : IEvent
	{
		private readonly IEventSubscriber<TEvent> _subscriber = subscriber;
		public int Priority => _subscriber.Priority;

		public void Handle(IEvent evnt)
		{
			_subscriber.Handle((TEvent)evnt);
		}
	}

}