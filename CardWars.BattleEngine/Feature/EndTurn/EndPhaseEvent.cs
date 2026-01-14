using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Feature.EndTurn;

public class EndPhaseEvent : IEvent
{
	public TurnPhase Phase;
}

public class EndPhaseEventHandler : IEventHandler<EndPhaseEvent>
{
	public void Handle(IServiceContainer serviceContainer, EndPhaseEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new EndPhaseResolver(request));
	}
}