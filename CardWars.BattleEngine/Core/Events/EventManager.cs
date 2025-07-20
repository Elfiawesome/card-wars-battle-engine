namespace CardWars.BattleEngine.Core.Events;

public class EventManager
{
	public EventLink<OnUnitSummonedEventContext, OnUnitSummonedSubscriber> OnUnitSummoned = new();
}

public class OnUnitSummonedEventContext : EventContext
{

}

public class OnUnitSummonedSubscriber : EventSubscriber<OnUnitSummonedEventContext>
{
	public override void Handle(OnUnitSummonedEventContext context)
	{
	}
}