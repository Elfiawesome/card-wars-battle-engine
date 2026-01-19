using CardWars.BattleEngine;
using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngineVanilla.Event;

public class TestEvent : IEvent;

public class TestEventHandler : IEventHandler<TestEvent>
{
	public void Handle(Transaction context, TestEvent request)
	{
		Console.WriteLine("Test Event Raised!");
		context.RaiseEvent(new TestEvent());
	}
}