using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class AttachDeckToPlayerBlock(
	EntityId PlayerId,
	EntityId DeckId
) : IBlock;

public class AttachDeckToPlayerBlockHandler : IBlockHandler<AttachDeckToPlayerBlock>
{
	public void Handle(GameState context, AttachDeckToPlayerBlock request)
	{
		var player = context.Get<Player>(request.PlayerId);
		var deck = context.Get<Deck>(request.DeckId);
		if (player == null || deck == null) { return; }
		
		deck.OwnerPlayerId = request.PlayerId;
		player.DeckIds.Add(request.DeckId);
	}
}