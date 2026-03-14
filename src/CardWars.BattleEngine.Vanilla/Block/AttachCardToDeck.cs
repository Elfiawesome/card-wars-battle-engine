using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachCardToDeckBlock(
	[property: DataTag] EntityId DeckId,
	[property: DataTag] EntityId CardId
) : IBlock;

public class AttachCardToDeckBlockHandler : IBlockHandler<AttachCardToDeckBlock>
{
	public void Handle(GameState context, AttachCardToDeckBlock request)
	{
		if (context.Require<Deck>(request.DeckId) is not { } deck) return;
		if (context.Require<GenericCard>(request.CardId) is not { } card) return;

		if (deck.CardIds.Contains(request.CardId)) { return; }
		deck.CardIds.Add(request.CardId);
	}
}