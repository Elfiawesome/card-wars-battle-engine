using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Features;

[DataTagType()]
public class PlayerDefeatedEvent : IEvent
{
	[DataTag] public EntityId PlayerId { get; set; }
	[DataTag] public int Team { get; set; }
}

public class PlayerDefeatedEventHandler : IEventHandler<PlayerDefeatedEvent>
{
	public void Handle(Transaction context, PlayerDefeatedEvent request)
	{
		if (context.State.Require<Player>(request.PlayerId) is not { } player) return;

		var batch = new BlockBatch([]);
		AddDestroyBoardBlocks(batch, context.State, player);
		if (batch.Blocks.Count > 0) context.ApplyBlockBatch(batch);
	}

	private static void AddDestroyBoardBlocks(BlockBatch batch, GameState state, Player player)
	{
		foreach (var battlefieldId in player.BattlefieldIds.ToList())
		{
			if (state.Get<Battlefield>(battlefieldId) is not { } battlefield) continue;

			foreach (var unitSlotId in battlefield.UnitSlotIds.ToList())
			{
				if (state.Get<UnitSlot>(unitSlotId) is not { } unitSlot) continue;

				if (unitSlot.HoldingCardId is { } cardId)
				{
					batch.Blocks.Add(new DetachCardFromUnitSlotBlock(unitSlot.Id, cardId));
					batch.Blocks.Add(new AttachCardToGraveyardBlock(player.Id, cardId));
				}

				batch.Blocks.Add(new DetachUnitSlotFromBattlefieldBlock(battlefield.Id, unitSlot.Id));
				batch.Blocks.Add(new RemoveEntityBlock(unitSlot.Id));
			}

			foreach (var heroId in battlefield.HeroCardIds.ToList())
			{
				batch.Blocks.Add(new DetachHeroCardFromBattlefieldBlock(battlefield.Id, heroId));
				batch.Blocks.Add(new RemoveEntityBlock(heroId));
			}

			batch.Blocks.Add(new DetachBattlefieldFromPlayerBlock(player.Id, battlefield.Id));
			batch.Blocks.Add(new RemoveEntityBlock(battlefield.Id));
		}
	}
}
