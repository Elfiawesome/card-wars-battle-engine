using System.Xml.Serialization;
using CardWars.BattleEngine.Core.Events;

public class EventContext
{

}

public class EventSubscriberGrouped<TContext, TIdentifer>
	where TContext : EventContext
	where TIdentifer : notnull
{
	public virtual void Handle(TIdentifer id, TContext context)
	{

	}
}


public class EventLinkGrouped<TEventContext, TEventSubscriber, TIdentifier>
	where TEventContext : EventContext
	where TEventSubscriber : EventSubscriberGrouped<TEventContext, TIdentifier>
	where TIdentifier : notnull
{
	private List<TEventSubscriber> _allSubscribers = [];
	private Dictionary<TIdentifier, List<TEventSubscriber>> _groupedSubscribers = [];
	private Dictionary<TEventSubscriber, List<TIdentifier>> _subscribersToMapping = []; // Amazing name I know

	public void Subscribe(TIdentifier id, TEventSubscriber subscriber)
	{
		if (_groupedSubscribers.ContainsKey(id))
		{
			_groupedSubscribers[id].Add(subscriber);
		}
		else
		{
			_groupedSubscribers[id] = [subscriber];
		}

		if (_subscribersToMapping.ContainsKey(subscriber))
		{
			_subscribersToMapping[subscriber].Add(id);
		}
		else
		{
			_subscribersToMapping[subscriber] = [id];
		}
	}

	public void Subscribe(TEventSubscriber subscriber)
	{
		if (_allSubscribers.Contains(subscriber))
		{
			_allSubscribers.Add(subscriber);
		}
	}

	public void Unsubscribe(TEventSubscriber subscriber)
	{
		if (_allSubscribers.Contains(subscriber))
		{
			_allSubscribers.Remove(subscriber);
		}

		if (_subscribersToMapping.ContainsKey(subscriber))
		{
			foreach (var id in _subscribersToMapping[subscriber])
			{
				_groupedSubscribers[id].Remove(subscriber);
			}

			_subscribersToMapping[subscriber].Clear();
			_subscribersToMapping.Remove(subscriber);
		}
	}

	public void Raise(TIdentifier id, TEventContext context)
	{
		if (_groupedSubscribers.ContainsKey(id))
		{
			foreach (var subscriber in _groupedSubscribers[id])
			{
				subscriber.Handle(id, context);
			}
		}

		foreach (var subscriber in _allSubscribers)
		{
			subscriber.Handle(id, context);
		}
	}
}