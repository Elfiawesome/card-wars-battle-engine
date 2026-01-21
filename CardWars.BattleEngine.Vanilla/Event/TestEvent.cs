using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngine.Vanilla.Event;

public class TestEvent() : IEvent;

public class TestEventHandler : IEventHandler<TestEvent>
{
	public void Handle(Transaction context, TestEvent request)
	{
	}
}