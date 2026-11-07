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
public record struct AttackHeroRequestInput(
	[property: DataTag] List<EntityId> AttackerSlotIds,
	[property: DataTag] EntityId HeroCardId
) : IInput;

public class AttackHeroRequestInputHandler : IInputHandler<AttackHeroRequestInput>
{
	public void Handle(InputContext context, AttackHeroRequestInput request)
	{
		var state = context.Transaction.State;

		if (state.Require<GenericCard>(request.HeroCardId) is not { IsHero: true } hero)
		{
			Log.Warn($"Hero attack rejected: [{request.HeroCardId}] is not a hero");
			return;
		}

		var validator = new BattleActionValidator(context.Transaction.Registry);
		var queuedAttackers = new HashSet<EntityId>();

		foreach (var attackerSlotId in request.AttackerSlotIds ?? [])
		{
			if (!queuedAttackers.Add(attackerSlotId)) continue;
			if (state.Require<UnitSlot>(attackerSlotId) is not { HoldingCardId: { } attackerCardId }) continue;
			if (state.Require<GenericCard>(attackerCardId) is not { } attackerCard) continue;

			if (!validator.TryResolveAttack(state, context.PlayerId, attackerCard, out _, out var reason))
			{
				Log.Warn($"Hero attack rejected: {reason}");
				continue;
			}

			if (!validator.CanTargetHero(state, attackerCard, hero, out reason))
			{
				Log.Warn($"Hero attack rejected: {reason}");
				continue;
			}

			context.Transaction.QueueEvent(new AttackHeroRequestEvent()
			{
				AttackerId = attackerCard.Id,
				HeroCardId = hero.Id
			});
		}
	}
}

[DataTagType()]
public class AttackHeroRequestEvent : IEvent
{
	[DataTag] public EntityId AttackerId { get; set; }
	[DataTag] public EntityId HeroCardId { get; set; }
	[DataTag] public bool IsCancelled { get; set; }
}

public class AttackHeroRequestEventHandler : IEventHandler<AttackHeroRequestEvent>
{
	public void Handle(Transaction context, AttackHeroRequestEvent request)
	{
		if (request.IsCancelled) return;

		context.QueueEvent(new HeroAttackEvent()
		{
			AttackerId = request.AttackerId,
			HeroCardId = request.HeroCardId
		});
	}
}

[DataTagType()]
public class HeroAttackEvent : IEvent
{
	[DataTag] public EntityId AttackerId { get; set; }
	[DataTag] public EntityId HeroCardId { get; set; }
}

public class HeroAttackEventHandler : IEventHandler<HeroAttackEvent>
{
	public void Handle(Transaction context, HeroAttackEvent request)
	{
		var state = context.State;

		if (state.Require<GenericCard>(request.AttackerId) is not { } attackerCard) return;
		if (state.Require<GenericCard>(request.HeroCardId) is not { IsHero: true } hero) return;

		if (AttackTargetResolver.GetOwnerPlayerId(state, attackerCard) is not { } ownerId) return;

		var validator = new BattleActionValidator(context.Registry);
		if (!validator.TryResolveAttack(state, ownerId, attackerCard, out var attack, out var reason))
		{
			Log.Warn($"Hero attack rejected: {reason}");
			return;
		}

		if (!validator.CanTargetHero(state, attackerCard, hero, out reason))
		{
			Log.Warn($"Hero attack rejected: {reason}");
			return;
		}

		var damage = AttackDamageResolver.GetOriginDamage(attackerCard, attack);
		var batch = new BlockBatch([])
		{
			AnimationId = SharedIds.Animations.Attack.ToString(),
			AnimationArgs = new CompoundTag()
				.Set(SharedIds.Animations.Args.Attacker, new GuidTag(attackerCard.Id.Value))
				.Set(SharedIds.Animations.Args.Target, new GuidTag(hero.Id.Value))
		};

		if (attack.ChargeCost > 0)
		{
			batch.Blocks.Add(new SetCardDataBlock(
				attackerCard.Id,
				"charge",
				new IntTag(attackerCard.Charge - attack.ChargeCost)));
		}

		context.QueueEvent(new HeroTakeDamageEvent()
		{
			HeroCardId = hero.Id,
			Amount = damage,
			SourceId = attackerCard.Id
		});

		context.ApplyBlockBatch(batch);
	}
}

[DataTagType()]
public class HeroTakeDamageEvent : IEvent
{
	[DataTag] public EntityId HeroCardId { get; set; }
	[DataTag] public int Amount { get; set; }
	[DataTag] public EntityId? SourceId { get; set; }
}

public class HeroTakeDamageEventHandler : IEventHandler<HeroTakeDamageEvent>
{
	public void Handle(Transaction context, HeroTakeDamageEvent request)
	{
		if (context.State.Require<GenericCard>(request.HeroCardId) is not { IsHero: true } hero) return;

		var hrtDamage = GenericCard.AtkToHrt(request.Amount);
		var hrt = new IntTag(hero.Hrt - hrtDamage);
		var batch = new BlockBatch([new SetCardDataBlock(hero.Id, "hrt", hrt)])
		{
			AnimationId = SharedIds.Animations.TakeDamage.ToString(),
			AnimationArgs = new CompoundTag().Set(SharedIds.Animations.Args.CardId, new GuidTag(hero.Id.Value))
		};
		context.ApplyBlockBatch(batch);
	}
}
