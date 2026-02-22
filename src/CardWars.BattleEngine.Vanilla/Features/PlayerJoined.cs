using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Block;

namespace CardWars.BattleEngine.Vanilla.Features;

public class PlayerJoinedEvent() : IEvent
{
	public EntityId PlayerId;
};

public class PlayerJoinedEventHandler : IEventHandler<PlayerJoinedEvent>
{
	public void Handle(Transaction context, PlayerJoinedEvent request)
	{
		var batch = new BlockBatch([]);

		var deckId = EntityId.New();
		batch.Blocks.Add(new InstantiateDeckBlock(deckId));
		batch.Blocks.Add(new AttachDeckToPlayer(request.PlayerId, deckId));
		
		var cardId = EntityId.New();
		batch.Blocks.Add(new InstantiateCardBlock(cardId));

		context.ApplyBlockBatch(batch);
	}
}