using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Events.Events;

public class OnUnitSummonedEventContext : EventContext
{
	UnitId UnitId;
}

public class OnUnitSummonedSubscriber : EventSubscriber<OnUnitSummonedEventContext>
{
	public override void Handle(OnUnitSummonedEventContext context)
	{
		
	}
}