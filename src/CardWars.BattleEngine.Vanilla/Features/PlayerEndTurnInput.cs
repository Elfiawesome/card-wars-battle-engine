using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;

namespace CardWars.BattleEngine.Vanilla.Features;

public record struct PlayerEndTurnInput(
) : IInput;

public class PlayerEndTurnInputHandler : IInputHandler<PlayerJoinedInput>
{
	public void Handle(Transaction context, PlayerJoinedInput request)
	{
		Console.WriteLine("Oh player ending turn?");
	}
}