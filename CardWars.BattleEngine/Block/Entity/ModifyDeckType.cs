using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyDeckTypeBlock(
	DeckId DeckId,
	DeckType DeckType
) : IBlock;

public class ModifyDeckTypeBlockHandler : IBlockHandler<ModifyDeckTypeBlock>
{
	public bool Handle(IServiceContainer service, ModifyDeckTypeBlock request)
	{
		if (!service.State.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }

		deck.DeckType = request.DeckType;

		return true;
	}
}