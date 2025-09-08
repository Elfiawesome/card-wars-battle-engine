using CardWars.BattleEngine.Core.Events.EventContexts;

namespace CardWars.BattleEngine.Core.Events;

public class EventManager
{
	public EventSignal<OnUnitSummonedEventContext, EventOutcome> OnUnitSummoned = new();
	public EventSignal<OnUnitSummonedEventContext, EventOutcome> OnUnitUnsummoned = new();
}