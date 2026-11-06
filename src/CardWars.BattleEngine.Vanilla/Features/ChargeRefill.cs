using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class ChargeRefillEvent : IEvent
{
	[DataTag] public EntityId CardId { get; set; }
	[DataTag] public int Amount { get; set; }
	[DataTag] public bool IsCancelled { get; set; }
}

public class ChargeRefillEventHandler : IEventHandler<ChargeRefillEvent>
{
	public void Handle(Transaction context, ChargeRefillEvent request)
	{
		if (request.IsCancelled) return;
		if (context.State.Require<GenericCard>(request.CardId) is not { IsUnit: true, OwnerUnitSlotId: not null } card) return;

		context.ApplyBlockBatch(new BlockBatch([
			new SetCardDataBlock(card.Id, "charge", new IntTag(request.Amount))
		]));
	}
}
