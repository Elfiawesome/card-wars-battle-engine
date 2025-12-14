using CardWars.BattleEngine.Event.Turn;

namespace CardWars.BattleEngine.Input.Turn;

public record EndTurnInput() : IInput;

public class EndTurnInputHandler : IInputHandler<EndTurnInput>
{
	public void Handle(InputHandlerContext context, EndTurnInput request)
	{
		context.ServiceContainer.EventService.Raise(new RequestEndTurnEvent());
	}
}