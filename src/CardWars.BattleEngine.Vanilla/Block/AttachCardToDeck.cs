using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class AttachCardToDeckBlock(
	EntityId DeckId,
	EntityId CardId
) : IBlock;

public class AttachDeckToPlayerHandler : IBlockHandler<AttachCardToDeckBlock>
{
	public void Handle(GameState context, AttachCardToDeckBlock request)
	{
		var deck = context.Get<Deck>(request.DeckId);
		var card = context.Get<GenericCard>(request.CardId);
		if (deck == null || card == null) { return; }

		if (deck.CardIds.Contains(request.CardId)) { return; }
		deck.CardIds.Add(request.CardId);
	}
}