using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Rules;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

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
		var state = context.Transaction.State;

		if (state.Require<GenericCard>(request.CardId) is not { } card)
		{
			Log.Warn($"Card play rejected: card [{request.CardId}] was not found");
			return;
		}

		var validator = new BattleActionValidator(context.Transaction.Registry);
		if (!validator.CanPlayCard(state, context.PlayerId, card, request.TargetEntityId, out var reason))
		{
			Log.Warn($"Card play rejected: {reason}");
			return;
		}

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