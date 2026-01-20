using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngineVanilla.Event;

namespace CardWars.BattleEngineVanilla.Behaviour;

public class TestBehehaviour : IBehaviour
{
	public Type? ListeningEventType { get; set; } = typeof(TestEvent);
	public int Priority { get; set; } = 0;

	public void OnEvent(IEvent evnt)
	{
		Console.WriteLine("This behaviour activated");
		if (evnt is TestEvent testEvent)
		{
			testEvent.SomeNumber += 1;
		}
	}
}