using CardWars.BattleEngine.Core.Events.Events;

namespace CardWars.BattleEngine.Core.Events;

public class EventManager
{
	public EventLink<OnUnitSummonedEventContext, OnUnitSummonedSubscriber> OnUnitSummoned = new();
}

