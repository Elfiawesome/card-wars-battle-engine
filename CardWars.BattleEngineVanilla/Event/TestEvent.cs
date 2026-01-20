using CardWars.BattleEngine;
using CardWars.BattleEngine.Event;

namespace CardWars.BattleEngineVanilla.Event;

public class TestEvent : IEvent
{
	public int SomeNumber = 0;
	public string SomeString = "";
}

public class TestEventHandler : IEventHandler<TestEvent>
{
	public void Handle(Transaction context, TestEvent request)
	{
		Console.WriteLine("Test Event Raised and Resolving! with " + request.SomeNumber + " & '" + request.SomeString + "'");
		// context.QueueEvent(request);
	}
}