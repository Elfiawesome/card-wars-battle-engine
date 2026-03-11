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
		var deck = context.Get<Deck>(request.DeckId);
		var card = context.Get<GenericCard>(request.CardId);
		if (deck == null || card == null) { return; }

		if (deck.CardIds.Contains(request.CardId)) { return; }
		deck.CardIds.Add(request.CardId);
	}
}