using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine.Event;

public class EventService(BattleEngine engine)
{
	private readonly BattleEngine _engine = engine;
	public void Raise(IEvent evnt, EventResponse eventResponses)
	{
		var allListeners = getAllActiveEventListeners();

		allListeners.Sort((a, b) => b.EventPriority.CompareTo(a.EventPriority));

		foreach (var listener in allListeners)
		{
			listener.OnGameEvent(_engine, evnt, eventResponses);
		}
	}

	public List<IEventListener> getAllActiveEventListeners()
	{
		var list = new List<IEventListener>();
		list.AddRange(_engine.EntityService.Players.Values);
		list.AddRange(_engine.EntityService.Battlefields.Values);
		list.AddRange(_engine.EntityService.UnitCards.Values);
		list.AddRange(_engine.EntityService.UnitSlots.Values);
		list.AddRange(_engine.EntityService.Abilities.Values);
		return list;
	}
}