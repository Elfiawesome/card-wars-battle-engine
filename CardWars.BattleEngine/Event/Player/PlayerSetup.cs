using CardWars.BattleEngine.Resolver.Player;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Event.Player;

public class PlayerSetupEvent : IEvent
{
	public PlayerId PlayerId;
	public int BattlefieldCount = 1;
	public int UnitSlotCount = 4;
}

public class PlayerSetupEventHandler : IEventHandler<PlayerSetupEvent>
{
	public void Handle(IServiceContainer serviceContainer, PlayerSetupEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new PlayerSetupResolver(request));
	}
}