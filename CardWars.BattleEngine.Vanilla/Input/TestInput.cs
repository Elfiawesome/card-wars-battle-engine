using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Vanilla.Event;

namespace CardWars.BattleEngine.Vanilla.Input;

public record struct TestInput() : IInput;

public class TestInputHandler : IInputHandler<TestInput>
{
	public void Handle(Transaction context, TestInput request)
	{
		context.QueueEvent(new TestEvent());
	}
}