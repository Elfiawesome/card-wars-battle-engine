using CardWars.BattleEngine.Resolver.Player;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Event.Player;

public class PlayerJoinedEvent : IEvent
{
	public PlayerId PlayerId;
}

public class PlayerJoinedEventHandler : IEventHandler<PlayerJoinedEvent>
{
	public void Handle(IServiceContainer serviceContainer, PlayerJoinedEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new PlayerJoinedResolver(request));
	}
}