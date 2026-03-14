using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public record struct UseCardRequestInput(
	[property: DataTag] EntityId CardId,
	[property: DataTag] EntityId? TargetEntityId
) : IInput;

public class UseCardRequestInputHandler : IInputHandler<UseCardRequestInput>
{
	public void Handle(InputContext context, UseCardRequestInput request)
	{
		context.Transaction.QueueEvent(new UseCardRequestEvent() { CardId = request.CardId, TargetEntityId = request.TargetEntityId });
	}
}

[DataTagType()]
public class UseCardRequestEvent() : IEvent
{
	[DataTag] public EntityId CardId { get; set; }
	[DataTag] public EntityId? TargetEntityId { get; set; }
	[DataTag] public bool IsCancelled { get; set; }
};

public class UseCardRequestEventHandler : IEventHandler<UseCardRequestEvent>
{
	public void Handle(Transaction context, UseCardRequestEvent request)
	{
		// Uh do nothing I guess. Since the behaviours of the individual cards will handle them
	}
}