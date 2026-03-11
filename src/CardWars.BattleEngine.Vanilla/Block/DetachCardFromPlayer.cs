using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachCardFromPlayerBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId CardId
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