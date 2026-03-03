using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;

namespace CardWars.BattleEngine.Vanilla.Behaviour;

public class SummonUnitCardToUnitSlotBehaviour : Behaviour<UseCardRequestEvent>
{
	public override int Priority => 0;

	protected override BehaviourResult Start(UseCardRequestEvent evnt, BehaviourContext context)
	{
		var batch = new BlockBatch([]);
		// Only care if its my behaviour that I'm listening to
		if (evnt.CardId == context.OwnerEntityId)
		{
			if (context.Owner is GenericCard card)
			{
				if (evnt.TargetEntityId != null && evnt.TargetEntityId != Guid.Empty)
				{
					var unitSlot = context.State.Get<UnitSlot>(evnt.TargetEntityId ?? Guid.Empty);
					if (unitSlot != null)
					{
						batch.Blocks.Add(new DetachCardFromPlayerBlock(card.OwnerPlayerId ?? Guid.Empty, card.Id));
						batch.Blocks.Add(new AttachCardToUnitSlotBlock(unitSlot.Id, card.Id));
						Console.WriteLine("We want to summon this unit " + context.OwnerEntityId + " to slot " + evnt.TargetEntityId);
					}
				}
			}
		}

		context.CommitBlocks(batch);
		context.CommitStagedBlocks();

		return BehaviourResult.Complete;
	}
}