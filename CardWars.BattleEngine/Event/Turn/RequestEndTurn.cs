using CardWars.BattleEngine.Resolver.Turn;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Event.Turn;

public class RequestEndTurnEvent : IEvent
{
	public PlayerId PlayerId;
}

public class RequestEndTurnEventEventHandler : IEventHandler<RequestEndTurnEvent>
{
	public void Handle(IServiceContainer serviceContainer, RequestEndTurnEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new RequestEndTurnResolver(request));
	}
}