using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Event;

public interface IEventHandler<TEvent> : IRequestHandler<IServiceContainer, TEvent>
	where TEvent : IEvent;