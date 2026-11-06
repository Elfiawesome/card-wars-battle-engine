using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachCardToGraveyardBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId CardId
) : IBlock;

public class AttachCardToGraveyardBlockHandler : IBlockHandler<AttachCardToGraveyardBlock>
{
	public void Handle(GameState context, AttachCardToGraveyardBlock request)
	{
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		if (context.Require<GenericCard>(request.CardId) is not { } card) return;

		if (!player.GraveyardCardIds.Contains(card.Id))
			player.GraveyardCardIds.Add(card.Id);
	}
}
