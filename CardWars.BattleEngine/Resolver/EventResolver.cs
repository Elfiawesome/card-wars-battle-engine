using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Resolver;

public abstract class EventResolver<T>(T evnt) : Resolver
	where T : IEvent
{
	public T Event = evnt;
}