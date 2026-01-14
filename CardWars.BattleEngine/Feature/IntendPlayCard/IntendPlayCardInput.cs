using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Feature.IntendPlayCard;

public record IntendPlayCardInput(DeckId DeckId, int HandPos, TargetPlay TargetPlay, IStateId? TargetId = null) : IInput;

public class IntendPlayCardInputHandler : IInputHandler<IntendPlayCardInput>
{
	public void Handle(InputHandlerContext context, IntendPlayCardInput request)
	{
		context.ServiceContainer.EventService.Raise(new IntendPlayCardEvent() { 
			PlayerId = context.PlayerId, 
			PlayingCardHandPos = request.HandPos,
			TargetPlay = request.TargetPlay,
			TargetId = request.TargetId
		});
	}
}