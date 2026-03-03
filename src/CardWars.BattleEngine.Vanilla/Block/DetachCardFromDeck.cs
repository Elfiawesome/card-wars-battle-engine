using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class DetachCardFromDeckBlock(
	EntityId DeckId,
	EntityId CardId
) : IBlock;

public class DetachCardFromDeckBlockHandler : IBlockHandler<DetachCardFromDeckBlock>
{
	public void Handle(GameState context, DetachCardFromDeckBlock request)
	{
		var deck = context.Get<Deck>(request.DeckId);
		var card = context.Get<GenericCard>(request.CardId);
		if (deck == null || card == null) { return; }

		if (deck.CardIds.Contains(request.CardId))
		{
			deck.CardIds.Remove(request.CardId);
		}
	}
}