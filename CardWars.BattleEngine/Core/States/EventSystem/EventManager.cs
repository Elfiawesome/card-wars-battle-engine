using CardWars.BattleEngine.Core.States.EventSystem.EventContexts;

namespace CardWars.BattleEngine.Core.States.EventSystem;

public class EventManager
{
	public EventSignal<OnUnitSummonedEventContext, EventOutcome> OnUnitSummoned = new();
	public EventSignal<OnUnitSummonedEventContext, EventOutcome> OnUnitUnsummoned = new();
}