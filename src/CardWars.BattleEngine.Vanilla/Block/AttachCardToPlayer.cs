using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class AttachCardToPlayerBlock(
	EntityId DeckId,
	EntityId PlayerId
) : IBlock;

public class AttachCardToPlayerBlockHandler : IBlockHandler<AttachCardToPlayerBlock>
{
	public void Handle(GameState context, AttachCardToPlayerBlock request)
	{
		var deck = context.Get<Deck>(request.DeckId);
		var player = context.Get<Player>(request.PlayerId);
		if (deck == null || player == null) { return; }

		if (player.HandCardIds.Contains(request.CardId)) { return; }
		deck.CardIds.Add(request.CardId);
	}
}