using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
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
				context.Logger.Warn($"Attack rejected: {reason}");
				continue;
			}

			if (!validator.CanTargetUnit(state, attackerCard, attack, targetSlot.Id, out reason))
			{
				context.Logger.Warn($"Attack rejected: {reason}");
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
		if (attackerCard.OwnerUnitSlotId is not { } attackerOwnerUnitSlotId) return;
		if (state.Require<UnitSlot>(attackerOwnerUnitSlotId) is not { } attackerUnitSlot) return;
		if (state.Require<GenericCard>(request.TargetId) is not { } targetCard) return;
		if (state.GetParent<Battlefield>(request.TargetId) is not { } battlefield) return;
		if (targetCard.OwnerUnitSlotId is not { } targetSlotId) { context.Logger.Error("Target Card is not on any OwnerUnitSlotId"); return; }

		if (state.GetParent<Player>(attackerCard)?.Id is not { } ownerId) return;

		var validator = new BattleActionValidator(context.Registry);
		if (!validator.TryResolveAttack(state, ownerId, attackerCard, out var attack, out var reason))
		{
			context.Logger.Warn($"Attack rejected: {reason}");
			return;
		}

		if (!validator.CanTargetUnit(state, attackerCard, attack, targetSlotId, out reason))
		{
			context.Logger.Warn($"Attack rejected: {reason}");
			return;
		}

		var batch = new BlockBatch([])
		{
			AnimationId = SharedIds.Animations.Attack.ToString(),
			AnimationArgs = new CompoundTag()
				.Set(SharedIds.Animations.Args.Attacker, new GuidTag(attackerCard.Id.Value))
				.Set(SharedIds.Animations.Args.Target, new GuidTag(targetCard.Id.Value))
		};

		if (attack.Definition.ChargeCost > 0)
		{
			batch.Blocks.Add(new SetCardDataBlock(
				attackerCard.Id,
				"charge",
				new IntTag(attackerCard.Charge - attack.Definition.ChargeCost)));
		}

		foreach (var unitSlotId in battlefield.UnitSlotIds)
		{
			if (state.Get<UnitSlot>(unitSlotId) is not { } unitSlot) continue;
			if (state.Get<UnitSlot>(targetSlotId) is not { } targetSlot) continue;
			if (unitSlot.HoldingCardId is not { } damagedUnitId) continue;

			if (attack.IsHit(targetSlot.Position, unitSlot.Position, out var spec))
			{
				context.QueueEvent(new UnitTakeDamageEvent()
				{
					Amt = spec.Resolve(attackerCard.Atk),
					DamagedUnitId = damagedUnitId,
					EntitySourceId = attackerCard.Id,
					IsSPAtk = attack.IsSpAtk
				});
			}
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