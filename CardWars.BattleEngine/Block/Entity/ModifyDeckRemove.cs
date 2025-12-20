using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyDeckRemoveBlock(
	DeckId DeckId,
	DeckDefinitionIdKey DeckDefinitionIdKey
) : IBlock;

public class ModifyDeckRemoveBlockHandler : IBlockHandler<ModifyDeckRemoveBlock>
{
	public bool Handle(IServiceContainer service, ModifyDeckRemoveBlock request)
	{
		if (!service.State.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }

		deck.DefinitionIds.Remove(request.DeckDefinitionIdKey);
		return true;
	}
}