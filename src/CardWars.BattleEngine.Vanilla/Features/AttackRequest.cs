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
		if (state.Turn.Phase != TurnPhase.Attacking) { Logger.Error("Tried to run a attack request not in attacking phase"); return; }

		// Check if Targeted Unit Slot is valid
		var targetSlot = state.Get<UnitSlot>(request.TargetSlotId);
		if (targetSlot == null) { return; }

		// Check if Targeted Unit is valid
		if (targetSlot.HoldingCardId == null) { return; }
		var targetCard = state.Get<GenericCard>((EntityId)targetSlot.HoldingCardId);
		if (targetCard == null) { return; }

		foreach (var attackerSlotId in request.AttackerSlotIds)
		{
			// Check if Attacker Unit Slot is valid
			var attackerSlot = state.Get<UnitSlot>(attackerSlotId);
			if (attackerSlot == null) { continue; }

			// Check if Attacker Unit is valid
			if (attackerSlot.HoldingCardId == null) { continue; }
			var attackerCard = state.Get<GenericCard>((EntityId)attackerSlot.HoldingCardId);
			if (attackerCard == null) { return; }

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
		var attackerCard = state.Get<GenericCard>(request.AttackerId);
		var targetCard = state.Get<GenericCard>(request.TargetId);
		if (attackerCard == null || targetCard == null) { return; }
		if (targetCard.OwnerUnitSlotId == null) { return; }


		// void HandleSPAtk()
		// {
		// 	Logger.Custom($"{context}");
		// }

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

			Logger.Custom($"{name.Value}");
			Logger.Custom($"{multiType.Value}");
			Logger.Custom($"{multiAmt.Value}");
			Logger.Custom($"{chargeCost.Value}");
			Logger.Custom($"{amt.Value}");

			var unitSlotsTargetted = MultiUnitSlotTargetSystem.ListUnitSlots(state, (EntityId)targetCard.OwnerUnitSlotId, multiType.Value);
			unitSlotsTargetted.ForEach((s) => Logger.Custom("-->" + s.ToString()));
		}

		AttackRules.CanTargetSlot(state, (EntityId)targetCard.OwnerUnitSlotId);
	}
}
