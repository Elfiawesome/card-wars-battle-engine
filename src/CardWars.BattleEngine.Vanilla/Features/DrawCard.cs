using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Vanilla.Features;

public class DrawCardEvent() : IEvent
{
};

public class DrawCardEventHandler : IEventHandler<DrawCardEvent>
{
	public void Handle(Transaction context, DrawCardEvent request)
	{
	}
}