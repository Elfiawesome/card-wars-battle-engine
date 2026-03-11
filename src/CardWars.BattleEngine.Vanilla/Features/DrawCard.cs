using CardWars.BattleEngine.Event;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class DrawCardEvent() : IEvent
{
};

public class DrawCardEventHandler : IEventHandler<DrawCardEvent>
{
	public void Handle(Transaction context, DrawCardEvent request)
	{
	}
}