using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Vanilla.Features;

public record struct PlayerEndTurnInput(
) : IInput;

public class PlayerEndTurnInputHandler : IInputHandler<PlayerEndTurnInput>
{
	public void Handle(Transaction context, PlayerEndTurnInput request)
	{
	}
}