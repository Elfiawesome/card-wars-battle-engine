using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Helpers;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public record struct AttackRequestInput(
	[property: DataTag] List<EntityId> AttackerSlotIds,
	[property: DataTag] EntityId TargetSlotId
) : IInput;

public class AttackRequestInputHandler : IInputHandler<AttackRequestInput>
{
	public void Handle(InputContext context, AttackRequestInput request)
	{
		var state = context.Transaction.State;

		// Check if only on attacking turn
		if (state.Turn.Phase != TurnPhase.Attacking) { Logger.Error("Attack request rejected: not in Attacking phase"); return; }

		// Check if Targeted Unit Slot is valid
		if (state.Require<UnitSlot>(request.TargetSlotId) is not { } targetSlot) return;

		// Check if Targeted Unit is valid
		if (targetSlot.HoldingCardId == null) { Logger.Error("Target Unit Slot does not have any holding card id"); return; }
		if (state.Require<GenericCard>((EntityId)targetSlot.HoldingCardId) is not { } targetCard) return;

		foreach (var attackerSlotId in request.AttackerSlotIds)
		{
			// Check if Attacker Unit Slot is valid
			if (state.Require<UnitSlot>(attackerSlotId) is not { } attackerSlot) continue;

			// Check if Attacker Unit is valid
			if (attackerSlot.HoldingCardId == null) { Logger.Error("One of Attacker's Unit Slot does not have any holding card id"); continue; }
			if (state.Require<GenericCard>((EntityId)attackerSlot.HoldingCardId) is not { } attackerCard) continue;

			context.Transaction.QueueEvent(new AttackRequestEvent() { AttackerId = attackerCard.Id, TargetId = targetCard.Id });
		}
	}
}

// Used if we want to cancel the attack request
[DataTagType()]
public class AttackRequestEvent : IEvent
{
	[DataTag] public EntityId AttackerId { get; set; }
	[DataTag] public EntityId TargetId { get; set; }
	[DataTag] public bool IsCancelled { get; set; } = false;
}

public class AttackRequestEventHandler : IEventHandler<AttackRequestEvent>
{
	public void Handle(Transaction context, AttackRequestEvent request)
	{
		if (request.IsCancelled) { return; }

		// Allow this event to modify like taunt
		var evnt = new TargetUnitEvent() { TargetId = request.TargetId };
		context.QueueEvent(evnt);

		// Running actual event based on the modified event
		context.QueueEvent(new UnitAttackEvent() { AttackerId = request.AttackerId, TargetId = evnt.TargetId });
	}
}

// Used by taunt abilities to modify. Does not have any event handlers since it is processed immediately when raised.
// We make this because we want this to be independant from the attack request. It should be used for any targetting
[DataTagType()]
public class TargetUnitEvent : IEvent
{
	[DataTag] public EntityId TargetId { get; set; }
}

public class TargetUnitEventHandler : IEventHandler<TargetUnitEvent>
{
	public void Handle(Transaction context, TargetUnitEvent request) { /* Not used */ }
}


// Post attack event
[DataTagType()]
public class UnitAttackEvent : IEvent
{
	[DataTag] public EntityId AttackerId { get; set; }
	[DataTag] public EntityId TargetId { get; set; }
}

public class UnitAttackEventHandler : IEventHandler<UnitAttackEvent>
{
	public void Handle(Transaction context, UnitAttackEvent request)
	{
		var state = context.State;

		// Check if Attacker & Target Unit are valid
		if (state.Require<GenericCard>(request.AttackerId) is not { } attackerCard) return;
		if (state.Require<GenericCard>(request.TargetId) is not { } targetCard) return;
		if (targetCard.OwnerUnitSlotId == null) { Logger.Error("Target Card is not on any OwnerUnitSlotId"); return; }

		// Check if the attacker has SP Atk
		for (int i = 0; i < attackerCard.SpAtk.Count; i++)
		{
			var spAtk = attackerCard.SpAtk.Get<CompoundTag>(i);
			if (spAtk == null) { continue; }
			// Get SP Atk details
			var name = spAtk.Get<StringTag>("name") ?? new StringTag("Default");
			var multiType = spAtk.Get<StringTag>("multi_type") ?? new StringTag("");
			var multiAmt = spAtk.Get<IntTag>("multi_amt") ?? new IntTag(attackerCard.Atk);
			var chargeCost = spAtk.Get<IntTag>("charge_cost") ?? new IntTag(1);
			var amt = spAtk.Get<IntTag>("amt") ?? new IntTag(attackerCard.Atk);

			Logger.Debug($"SpAtk '{name.Value}': pattern={multiType.Value} amt={amt.Value} " +
				 $"multiAmt={multiAmt.Value} charge={chargeCost.Value}");

			var affected = SlotTargetResolver.Resolve(state, (EntityId)targetCard.OwnerUnitSlotId, multiType.Value);
			Logger.Debug($"Targeted slots: [{string.Join(", ", affected)}]");
		}

		AttackRules.CanTargetSlot(state, (EntityId)targetCard.OwnerUnitSlotId);
	}
}
