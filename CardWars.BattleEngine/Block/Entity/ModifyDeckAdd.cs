using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record ModifyDeckAddBlock(
	DeckId DeckId,
	string DefinitionId,
	DeckPosDefinitionId DeckPosDefinitionId
) : IBlock;

public class ModifyDeckAddBlockHandler : IBlockHandler<ModifyDeckAddBlock>
{
	public bool Handle(BattleEngine context, ModifyDeckAddBlock request)
	{
		if (!context.EntityService.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }
		
		deck.DefinitionIds.Add(request.DeckPosDefinitionId, request.DefinitionId);

		return true;
	}
}