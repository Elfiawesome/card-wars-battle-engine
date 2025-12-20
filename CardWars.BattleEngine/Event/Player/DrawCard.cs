using CardWars.BattleEngine.Resolver.Player;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Event.Player;

public class DrawCardEvent : IEvent
{
	public PlayerId PlayerId;
	public DeckId DeckId;
}

public class DrawCardEventHandler : IEventHandler<DrawCardEvent>
{
	public void Handle(IServiceContainer serviceContainer, DrawCardEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new DrawCardResolver(request));
	}
}