using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Feature.PlayerSetup;

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