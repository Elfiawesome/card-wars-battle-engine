using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Vanilla.Event;

namespace CardWars.BattleEngine.Vanilla.Input;

public record struct TestAdditionalInput() : IInput;

public class TestAdditionalInputHandler : IInputHandler<TestAdditionalInput>
{
	public void Handle(Transaction context, TestAdditionalInput request)
	{
		// Do nothing since its just for the behaviours
	}
}