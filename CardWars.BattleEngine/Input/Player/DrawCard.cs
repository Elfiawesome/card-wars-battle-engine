using CardWars.BattleEngine.Event.Player;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Input.Player;

public record DrawCardInput(DeckId DeckId) : IInput;

public class DrawCardInputHandler : IInputHandler<DrawCardInput>
{
	public void Handle(InputHandlerContext context, DrawCardInput request)
	{
		context.ServiceContainer.EventService.Raise(new DrawCardEvent() { PlayerId = context.PlayerId, DeckId = request.DeckId });
	}
}