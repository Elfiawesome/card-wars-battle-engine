using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Input;

public record DrawCardInput(
	DeckId DeckId
) : IInput;

public class DrawCardInputHandler : IInputHandler<DrawCardInput>
{
	public void Handle(InputHandlerContext context, DrawCardInput request)
	{
		
	}
}
