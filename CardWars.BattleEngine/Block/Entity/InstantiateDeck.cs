using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record InstantiateDeckBlock(
	DeckId DeckId
) : IBlock;

public class InstantiateDeckBlockHandler : IBlockHandler<InstantiateDeckBlock>
{
	public bool Handle(BattleEngine context, InstantiateDeckBlock request)
	{
		if (context.EntityService.Decks.ContainsKey(request.DeckId)) { return false; }
		context.EntityService.Decks[request.DeckId] = new Deck(request.DeckId);
		return true;
	}
}