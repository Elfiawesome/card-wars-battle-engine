using CardWars.Core.Request;

namespace CardWars.BattleEngine.Event;

public interface IEventHandler<TEvent> : IRequestHandler<Transaction, TEvent>
	where TEvent : IEvent;