using CardWars.BattleEngine;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngineVanilla.Event;

namespace CardWars.BattleEngineVanilla.Input;

public record struct TestInput : IInput;

public class TestInputHandler() : IInputHandler<TestInput>
{
	public void Handle(Transaction context, TestInput request)
	{
		Console.WriteLine("Handling Test Input!");
		context.QueueEvent(new TestEvent());
	}
}