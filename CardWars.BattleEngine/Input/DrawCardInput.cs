using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine.Input;

public record DrawCardInput(
	DeckId DeckId
) : IInput;

public class DrawCardInputHandler : IInputHandler<DrawCardInput>
{
	public void Handle(InputHandlerContext context, DrawCardInput request)
	{
		if (!context.Engine.EntityService.Players.TryGetValue(context.PlayerId, out var player)) { return; }

		if (context.Engine.TurnService.Phase != TurnService.PhaseType.Setup) { return; } // Can't draw in attacking phase
		if (player.UnitDeckId != request.DeckId) { return; } // If that deck is not ours, we can't draw it
		
		context.Engine.QueueResolver(new PlayerDrawCardResolver(context.PlayerId, request.DeckId));
	}
}
