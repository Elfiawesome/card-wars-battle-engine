using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class PtReplenishEvent : IEvent
{
	[DataTag] public EntityId PlayerId { get; set; }
	[DataTag] public int TargetPt { get; set; }
	[DataTag] public bool IsCancelled { get; set; }
}

public class PtReplenishEventHandler : IEventHandler<PtReplenishEvent>
{
	public void Handle(Transaction context, PtReplenishEvent request)
	{
		if (request.IsCancelled) return;
		if (context.State.Require<Player>(request.PlayerId) is not { } player) return;

		var amount = Math.Max(0, request.TargetPt - player.Pt);
		if (amount == 0) return;

		context.ApplyBlockBatch(new BlockBatch([
			new ModifyPlayerPtBlock(player.Id, amount)
		]));
	}
}
