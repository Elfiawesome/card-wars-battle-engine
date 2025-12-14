using CardWars.BattleEngine.Event.Player;

namespace CardWars.BattleEngine.Event;

public class EventService(IServiceContainer container) : Service(container)
{
	public void Raise(IEvent evnt)
	{
		foreach (var ability in ServiceContainer.State.Abilities)
		{
			// Intepret the ability event here...
		}

		// Example of an ability or something 
		if (evnt is PlayerSetupEvent playerSetupEvent)
		{
			playerSetupEvent.UnitSlotCount = 2;
		}

		// Do for units/slots/battlefields/status effects etc.

		// Then we raise any resolver for this event
		if (ServiceContainer.EventResolverDispatcher.Handlers.ContainsKey(evnt.GetType()))
		{
			ServiceContainer.EventResolverDispatcher.Handle(ServiceContainer, evnt);
		}
	}
}