using CardWars.BattleEngine.Resolver;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Input;

public record EndTurnInput() : IInput;

public class EndTurnInputHandler : IInputHandler
{
	public void Handle(InputHandlerContext context, EndTurnInput request)
	{
		context.Engine.QueueResolver(new EndTurnResolver());
	}
}
