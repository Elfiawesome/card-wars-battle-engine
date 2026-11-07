using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Rules;
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
		if (state.Require<UnitSlot>(request.TargetSlotId) is not { HoldingCardId: { } targetCardId } targetSlot) return;
		if (state.Require<GenericCard>(targetCardId) is null) return;

		var validator = new BattleActionValidator(context.Transaction.Registry);
		var queuedAttackers = new HashSet<EntityId>();

		foreach (var attackerSlotId in request.AttackerSlotIds ?? [])
		{
			if (!queuedAttackers.Add(attackerSlotId)) continue;
			if (state.Require<UnitSlot>(attackerSlotId) is not { HoldingCardId: { } attackerCardId }) continue;
			if (state.Require<GenericCard>(attackerCardId) is not { } attackerCard) continue;

			if (!validator.TryResolveAttack(state, context.PlayerId, attackerCard, out var attack, out var reason))
			{
				Log.Warn($"Attack rejected: {reason}");
				continue;
			}

			if (!validator.CanTargetUnit(state, attackerCard, attack, targetSlot.Id, out reason))
			{
				Log.Warn($"Attack rejected: {reason}");
				continue;
			}

			context.Transaction.QueueEvent(new AttackRequestEvent() { AttackerId = attackerCard.Id, TargetId = targetCardId });
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

		context.QueueEvent(new TargetUnitEvent()
		{
			AttackerId = request.AttackerId,
			TargetId = request.TargetId
		});
	}
}

[DataTagType()]
public class TargetUnitEvent : IEvent
{
	[DataTag] public EntityId AttackerId { get; set; }
	[DataTag] public EntityId TargetId { get; set; }
}

public class TargetUnitEventHandler : IEventHandler<TargetUnitEvent>
{
	public void Handle(Transaction context, TargetUnitEvent request)
	{
		if (request.AttackerId.IsNone || request.TargetId.IsNone) return;

		context.QueueEvent(new UnitAttackEvent()
		{
			AttackerId = request.AttackerId,
			TargetId = request.TargetId
		});
	}
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

		if (AttackTargetResolver.GetOwnerPlayerId(state, attackerCard) is not { } ownerId) return;

		var validator = new BattleActionValidator(context.Registry);
		if (!validator.TryResolveAttack(state, ownerId, attackerCard, out var attack, out var reason))
		{
			Log.Warn($"Attack rejected: {reason}");
			return;
		}

		if (!validator.CanTargetUnit(state, attackerCard, attack, targetSlotId, out reason))
		{
			Log.Warn($"Attack rejected: {reason}");
			return;
		}

		var damage = AttackDamageResolver.Resolve(state, attackerCard, attack, targetSlotId);
		var batch = new BlockBatch([])
		{
			AnimationId = SharedIds.Animations.Attack.ToString(),
			AnimationArgs = new CompoundTag()
				.Set(SharedIds.Animations.Args.Attacker, new GuidTag(attackerCard.Id.Value))
				.Set(SharedIds.Animations.Args.Target, new GuidTag(targetCard.Id.Value))
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
			AnimationArgs = new CompoundTag().Set(SharedIds.Animations.Args.CardId, new GuidTag(request.DamagedUnitId.Value))
		};
		context.ApplyBlockBatch(batch);
	}
}