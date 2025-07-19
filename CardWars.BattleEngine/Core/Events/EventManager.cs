namespace CardWars.BattleEngine.Core.Events;

public class EventManager
{
	public EventSignal<BaseEventReturn, BaseEventContext> OnUnitSummoned = new();
	public EventSignal<BaseEventReturn, BaseEventContext> OnUnitKilled = new();
	public EventSignal<BaseEventReturn, BaseEventContext> OnUnitDamaged = new();
}

public class EventSignal<TEventReturn, TEventContext> where TEventContext : BaseEventContext where TEventReturn : BaseEventReturn
{
	private List<EventResolver<TEventReturn, TEventContext>> _resolvers = new();

	public void Subscribe(EventResolver<TEventReturn, TEventContext> resolver)
	{
		_resolvers.Add(resolver);
	}

	public void Unsubscribe(EventResolver<TEventReturn, TEventContext> resolver)
	{
		_resolvers.Add(resolver);
	}

	public List<TEventReturn> Publish(TEventContext context)
	{
		var results = new List<TEventReturn>();
		foreach (var resolver in _resolvers)
		{
			results.Add(
				resolver.Resolve(context)
			);
		}
		return results;
	}
}

public abstract class BaseEventContext { }

public abstract class BaseEventReturn { }

public abstract class EventResolver<TEventReturn, TEventContext> where TEventContext : BaseEventContext where TEventReturn : BaseEventReturn
{
	public abstract TEventReturn Resolve(TEventContext context);
}