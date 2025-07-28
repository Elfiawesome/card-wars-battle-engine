using CardWars.BattleEngine.Core.Events.Events;
using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Events;

public class EventManager
{
	public EventLinkGrouped<OnUnitSummonedEventContext, OnUnitSummonedSubscriber, UnitId> OnUnitSummoned = new();

	// List of events to make...
	// OnUnitSummoned		EventLinkGrouped
	// OnUnitAttacked		EventLinkGrouped
	// OnUnitDamaged		EventLinkGrouped
	// 

	public EventManager()
	{
	}
}

