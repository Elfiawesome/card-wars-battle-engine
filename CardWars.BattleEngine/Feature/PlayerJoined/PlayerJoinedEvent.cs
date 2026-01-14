using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Feature.PlayerJoined;

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