using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Event;

public interface IEventResolverHandler<TEvent> : IRequestHandler<IServiceContainer, TEvent>
	where TEvent : IEvent;