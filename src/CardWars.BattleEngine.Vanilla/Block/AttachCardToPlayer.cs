using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachCardToPlayerBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId CardId
) : IBlock;

public class AttachCardToPlayerBlockHandler : IBlockHandler<AttachCardToPlayerBlock>
{
	public void Handle(GameState context, AttachCardToPlayerBlock request)
	{
		var player = context.Get<Player>(request.PlayerId);
		var card = context.Get<GenericCard>(request.CardId);
		if (player == null || card == null) { return; }

		if (player.HandCardIds.Contains(request.CardId)) { return; }
		player.HandCardIds.Add(request.CardId);
		card.OwnerPlayerId = request.PlayerId;
	}
}