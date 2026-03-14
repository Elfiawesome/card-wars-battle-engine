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
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		if (context.Require<GenericCard>(request.CardId) is not { } card) return;

		if (player.HandCardIds.Contains(request.CardId)) { return; }
		player.HandCardIds.Add(request.CardId);
		card.OwnerPlayerId = request.PlayerId;
	}
}