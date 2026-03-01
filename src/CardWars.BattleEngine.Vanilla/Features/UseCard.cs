using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Features;

public class UseCardEvent() : IEvent
{
	public EntityId CardId;
};

public class UseCardEventHandler : IEventHandler<UseCardEvent>
{
	public void Handle(Transaction context, UseCardEvent request)
	{

	}
}