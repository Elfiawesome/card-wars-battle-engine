using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Features;

public record struct UseCardRequestInput(
	EntityId CardId,
	EntityId? TargetEntityId
) : IInput;

public class UseCardRequestInputHandler : IInputHandler<UseCardRequestInput>
{
	public void Handle(InputContext context, UseCardRequestInput request)
	{
		context.Transaction.QueueEvent(new UseCardRequestEvent() { CardId = request.CardId, TargetEntityId = request.TargetEntityId });
	}
}

public class UseCardRequestEvent() : IEvent
{
	public EntityId CardId;
	public EntityId? TargetEntityId;
	public bool IsCancelled;
};

public class UseCardRequestEventHandler : IEventHandler<UseCardRequestEvent>
{
	public void Handle(Transaction context, UseCardRequestEvent request)
	{
		// Uh do nothing I guess. Since the behaviours of the individual cards will handle them
	}
}