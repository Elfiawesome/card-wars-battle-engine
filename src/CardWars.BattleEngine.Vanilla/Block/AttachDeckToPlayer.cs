using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class AttachDeckToPlayer(
	EntityId PlayerId,
	EntityId DeckId
) : IBlock;

public class AttachDeckToPlayerHandler : IBlockHandler<AttachDeckToPlayer>
{
	public void Handle(GameState context, AttachDeckToPlayer request)
	{
		var player = context.Get<Player>(request.PlayerId);
		var deck = context.Get<Deck>(request.PlayerId);
		if (player == null || deck == null) { return; }
		
		deck.OwnerPlayerId = request.PlayerId;
		player.DeckIds.Add(request.DeckId);
	}
}