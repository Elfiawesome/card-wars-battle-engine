using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record ModifyDeckRemoveBlock(
	DeckId DeckId,
	DeckPosDefinitionId DeckPosDefinitionId
) : IBlock;

public class ModifyDeckRemoveBlockHandler : IBlockHandler<ModifyDeckRemoveBlock>
{
	public bool Handle(BattleEngine context, ModifyDeckRemoveBlock request)
	{
		if (!context.EntityService.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }
		
		deck.DefinitionIds.Remove(request.DeckPosDefinitionId);
		return true;
	}
}