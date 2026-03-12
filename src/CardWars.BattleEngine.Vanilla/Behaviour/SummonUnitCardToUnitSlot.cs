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

		// Skip if the card being used is not this behaviours entity parent
		if (evnt.CardId != context.OwnerEntityId) return BehaviourResult.Complete;
		// Skip if the entity parent isnt even a card
		if (context.Owner is not GenericCard card) return BehaviourResult.Complete;

		if (evnt.TargetEntityId != null && evnt.TargetEntityId != Guid.Empty)
		{
			var unitSlot = context.State.Get<UnitSlot>(evnt.TargetEntityId ?? Guid.Empty);
			if (unitSlot != null && unitSlot.HoldingCardId == null)
			{
				batch.Blocks.Add(new DetachCardFromPlayerBlock(card.OwnerPlayerId ?? Guid.Empty, card.Id));
				batch.Blocks.Add(new AttachCardToUnitSlotBlock(unitSlot.Id, card.Id));
				Console.WriteLine("We want to summon this unit " + context.OwnerEntityId + " to slot " + evnt.TargetEntityId);
			}
		}

		context.StageBlocks(batch);
		context.CommitStagedBlocks();

		return BehaviourResult.Complete;
	}
}