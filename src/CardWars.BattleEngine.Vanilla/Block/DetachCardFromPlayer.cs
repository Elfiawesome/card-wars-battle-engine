using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class DetachCardFromPlayerBlock(
	EntityId PlayerId,
	EntityId CardId
) : IBlock;

public class DetachCardFromPlayerBlockHandler : IBlockHandler<DetachCardFromPlayerBlock>
{
	public void Handle(GameState context, DetachCardFromPlayerBlock request)
	{
		var player = context.Get<Player>(request.PlayerId);
		var card = context.Get<GenericCard>(request.CardId);
		if (player == null || card == null) { return; }

		if (player.HandCardIds.Contains(request.CardId))
		{
			player.HandCardIds.Remove(request.CardId);
			card.OwnerPlayerId = null;
		}
	}
}