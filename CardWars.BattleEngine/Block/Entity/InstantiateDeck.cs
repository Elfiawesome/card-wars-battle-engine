using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record struct InstantiateDeckBlock(
	DeckId DeckId
) : IBlock;

public class InstantiateDeckBlockHandler : IBlockHandler<InstantiateDeckBlock>
{
	public bool Handle(IServiceContainer service, InstantiateDeckBlock request)
	{
		if (service.State.Decks.ContainsKey(request.DeckId)) { return false; }
		service.State.Decks[request.DeckId] = new Deck(request.DeckId);
		return true;
	}
}