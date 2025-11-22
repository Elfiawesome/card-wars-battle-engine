using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine.Input;

public record EndTurnInput() : IInput;

public class EndTurnInputHandler : IInputHandler<EndTurnInput>
{
	public void Handle(InputHandlerContext context, EndTurnInput request)
	{
		context.Engine.QueueResolver(new EndTurnResolver());
	}
}
