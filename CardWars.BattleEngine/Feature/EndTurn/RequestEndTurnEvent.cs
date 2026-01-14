using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Feature.EndTurn;

public class RequestEndTurnEvent : IEvent
{
	public PlayerId PlayerId;
}

public class RequestEndTurnEventHandler : IEventHandler<RequestEndTurnEvent>
{
	public void Handle(IServiceContainer serviceContainer, RequestEndTurnEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new RequestEndTurnResolver(request));
	}
}