using CardWars.BattleEngine.Input;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public record struct EndTurnRequestInput() : IInput;

public class EndTurnRequestInputHandler : IInputHandler<EndTurnRequestInput>
{
	public void Handle(InputContext context, EndTurnRequestInput request)
	{
		var turn = context.Transaction.State.Turn;
		context.Transaction.QueueEvent(new TurnEndedEvent()
		{
			PlayerId = context.PlayerId,
			Phase = turn.Phase,
			TurnNumber = turn.TurnNumber
		});
	}
}
