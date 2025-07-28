using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Events.Events;

public class OnUnitSummonedEventContext : EventContext
{
	UnitId UnitId;
}

public class OnUnitSummonedSubscriber : EventSubscriberGrouped<OnUnitSummonedEventContext, UnitId>
{
	public override void Handle(UnitId id, OnUnitSummonedEventContext context)
	{
		
	}
}