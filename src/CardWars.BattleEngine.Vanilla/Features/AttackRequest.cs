using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;

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
		if (state.Turn.Phase != TurnPhase.Attacking) { Log.Error("Attack request rejected: not in Attacking phase"); return; }

		// Check if Targeted Unit Slot is valid
		if (state.Require<UnitSlot>(request.TargetSlotId) is not { } targetSlot) return;
		if (targetSlot.HoldingCardId is null) { Log.Error("Target Unit Slot does not have any holding card id"); return; }
		if (state.Require<GenericCard>(targetSlot.HoldingCardId.Value) is null) return;

		var resolver = new AttackResolver(context.Transaction.Registry);
		var queuedAttackers = new HashSet<EntityId>();

		foreach (var attackerSlotId in request.AttackerSlotIds ?? [])
		{
			if (!queuedAttackers.Add(attackerSlotId)) continue;

			// Check if Attacker Unit Slot is valid
			if (state.Require<UnitSlot>(attackerSlotId) is not { } attackerSlot) continue;

			// Check if Attacker Unit is valid
			if (attackerSlot.HoldingCardId is not { } attackerCardId) { Log.Error("One of Attacker's Unit Slot does not have any holding card id"); continue; }
			if (state.Require<GenericCard>(attackerCardId) is not { } attackerCard) continue;

			// The attacker must belong to the player making this input.
			if (AttackTargetResolver.GetOwnerPlayerId(state, attackerSlot) != context.PlayerId)
			{
				Log.Warn($"Attack rejected: slot [{attackerSlot.Id}] is not owned by player [{context.PlayerId}]");
				continue;
			}

			var attack = resolver.Resolve(attackerCard);
			if (!resolver.CanAfford(attackerCard, attack))
			{
				Log.Warn($"Attack rejected: card [{attackerCard.Id}] cannot afford [{attack.Name}]");
				continue;
			}

			if (!AttackTargetResolver.CanTarget(state, attackerCard, attack, targetSlot.Id))
			{
				Log.Warn($"Attack rejected: target [{targetSlot.Id}] is not valid for card [{attackerCard.Id}]");
				continue;
			}

			context.Transaction.QueueEvent(new AttackRequestEvent() { AttackerId = attackerCard.Id, TargetId = targetSlot.HoldingCardId.Value });
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
		if (targetCard.OwnerUnitSlotId is not { } targetSlotId) { Log.Error("Target Card is not on any OwnerUnitSlotId"); return; }

		var resolver = new AttackResolver(context.Registry);
		var attack = resolver.Resolve(attackerCard);
		if (!resolver.CanAfford(attackerCard, attack))
		{
			Log.Warn($"Tried to attack with insufficient charge for [{attack.Name}]");
			return;
		}

		if (!AttackTargetResolver.CanTarget(state, attackerCard, attack, targetSlotId))
		{
			Log.Warn($"Attack target [{targetSlotId}] is not valid for [{attack.Name}]");
			return;
		}

		var damage = AttackDamageResolver.Resolve(state, attackerCard, attack, targetSlotId);
		var batch = new BlockBatch([])
		{
			AnimationId = SharedIds.Animations.Attack.ToString(),
			AnimationArgs = new CompoundTag()
				.Set("attacker", new GuidTag(attackerCard.Id.Value))
				.Set("target", new GuidTag(targetCard.Id.Value))
		};

		if (attack.ChargeCost > 0)
		{
			batch.Blocks.Add(new SetCardDataBlock(
				attackerCard.Id,
				"charge",
				new IntTag(attackerCard.Charge - attack.ChargeCost)));
		}

		foreach (var (slotId, amount) in damage)
		{
			if (state.Get<UnitSlot>(slotId) is not { HoldingCardId: { } damagedCardId }) continue;

			context.QueueEvent(new UnitTakeDamageEvent()
			{
				Amt = amount,
				DamagedUnitId = damagedCardId,
				EntitySourceId = attackerCard.Id,
				IsSPAtk = attack.IsSpAtk
			});
		}

		context.ApplyBlockBatch(batch);
	}
}


public class UnitTakeDamageEvent : IEvent
{
	[DataTag] public int Amt { get; set; }
	[DataTag] public EntityId DamagedUnitId { get; set; }
	[DataTag] public EntityId? EntitySourceId { get; set; }
	[DataTag] public bool IsSPAtk { get; set; } = false;
}

public class UnitTakeDamageEventHandler : IEventHandler<UnitTakeDamageEvent>
{
	public void Handle(Transaction context, UnitTakeDamageEvent request)
	{
		if (context.State.Require<GenericCard>(request.DamagedUnitId) is not { } targetCard) return;
		var newHp = new IntTag(targetCard.Hp - request.Amt);
		var batch = new BlockBatch([new SetCardDataBlock(request.DamagedUnitId, "hp", newHp)])
		{
			AnimationId = SharedIds.Animations.TakeDamage.ToString(),
			AnimationArgs = new CompoundTag().Set("card_id", new GuidTag(request.DamagedUnitId.Value))
		};
		context.ApplyBlockBatch(batch);
	}
}