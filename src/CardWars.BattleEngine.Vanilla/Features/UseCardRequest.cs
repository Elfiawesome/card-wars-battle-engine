using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Targeting;
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

		if (card.OwnerPlayerId != context.PlayerId)
		{
			Log.Warn($"Card play rejected: card [{card.Id}] is not owned by player [{context.PlayerId}]");
			return;
		}

		if (state.Require<Player>(context.PlayerId) is not { } player)
		{
			Log.Warn($"Card play rejected: player [{context.PlayerId}] was not found");
			return;
		}

		if (!player.HandCardIds.Contains(card.Id))
		{
			Log.Warn($"Card play rejected: card [{card.Id}] is not in player [{player.Id}]'s hand");
			return;
		}

		if (card.Pt > player.Pt)
		{
			Log.Warn($"Card play rejected: card [{card.Id}] costs {card.Pt} Pt but player [{player.Id}] has {player.Pt}");
			return;
		}

		var targetFilter = new CardTargetFilterEvaluator(context.Transaction.Registry.DataDriven);
		if (!targetFilter.CanTarget(state, card, player.Id, request.TargetEntityId))
		{
			Log.Warn($"Card play rejected: target [{request.TargetEntityId}] is not valid for card [{card.Id}]");
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