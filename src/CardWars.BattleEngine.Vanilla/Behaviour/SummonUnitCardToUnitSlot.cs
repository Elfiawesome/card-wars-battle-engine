using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.BattleEngine.Vanilla.Rules;
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

		if (evnt.IsCancelled) return BehaviourResult.Complete;
		// Skip if the card being used is not this behaviours entity parent
		if (evnt.CardId != context.OwnerEntityId) return BehaviourResult.Complete;
		// Skip if the entity parent isnt even a card
		if (context.Owner is not GenericCard card) return BehaviourResult.Complete;
		if (card.OwnerPlayerId is not { } ownerId) return BehaviourResult.Complete;

		var validator = new BattleActionValidator(context.Registry);
		if (!validator.CanPlayCard(context.State, ownerId, card, evnt.TargetEntityId, out var reason))
		{
			Log.Warn($"Card play rejected: {reason}");
			return BehaviourResult.Complete;
		}

		if (evnt.TargetEntityId is { } targetId && targetId != EntityId.None)
		{
			var unitSlot = context.State.Get<UnitSlot>(targetId);
			if (unitSlot is { HoldingCardId: null })
			{
				batch.Blocks.Add(new ModifyPlayerPtBlock(ownerId, -card.Pt));
				batch.Blocks.Add(new DetachCardFromPlayerBlock(ownerId, card.Id));
				batch.Blocks.Add(new AttachCardToUnitSlotBlock(unitSlot.Id, card.Id));
				batch.AnimationId = SharedIds.Animations.PlayCard.ToString();
				batch.AnimationArgs.Set(SharedIds.Animations.Args.CardId, new GuidTag(card.Id.Value));
				batch.AnimationArgs.Set(SharedIds.Animations.Args.SlotId, new GuidTag(unitSlot.Id.Value));
				Log.Debug("We want to summon this unit " + context.OwnerEntityId + " to slot " + targetId);
			}
			else
			{
				Log.Warn("Could not resolve SummonUnitCardToUnitSlotBehaviour due to invalid UnitSlot target");
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