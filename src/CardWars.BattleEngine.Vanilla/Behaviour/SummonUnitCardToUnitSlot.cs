using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;

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

		if (evnt.TargetEntityId is { } targetId && targetId != EntityId.None)
		{
			var unitSlot = context.State.Get<UnitSlot>(targetId);
			if (unitSlot is { HoldingCardId: null })
			{
				batch.Blocks.Add(new DetachCardFromPlayerBlock(card.OwnerPlayerId ?? EntityId.None, card.Id));
				batch.Blocks.Add(new AttachCardToUnitSlotBlock(unitSlot.Id, card.Id));
				batch.AnimationId = SharedIds.Animations.PlayCard.ToString();
				batch.AnimationArgs.Set("card_id", new GuidTag(card.Id.Value));
				batch.AnimationArgs.Set("slot_id", new GuidTag(unitSlot.Id.Value));
				Log.Debug("We want to summon this unit " + context.OwnerEntityId + " to slot " + targetId);
			}
			else
			{
				Log.Warn("Could not resolve SummonUnitCardToUnitSlotBehaviour due to UnitSlot not being empty");
			}
		}
		else
		{
			Log.Warn("Could not resolve SummonUnitCardToUnitSlotBehaviour due to empty/null TargetEntityId");
		}

		context.StageBlocks(batch);

		return BehaviourResult.Complete;
	}
}