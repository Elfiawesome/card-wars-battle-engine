using CardWars.BattleEngine.Core.EventSystem.EventContexts;

namespace CardWars.BattleEngine.Core.EventSystem;

public class EventManager
{
	public EventSignal<OnUnitSummonedEventContext, EventOutcome> OnUnitSummoned = new();
	public EventSignal<OnUnitSummonedEventContext, EventOutcome> OnUnitUnsummoned = new();
}