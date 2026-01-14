using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyDeckAddBlock(
	DeckId DeckId,
	DeckDefinitionIdKey DeckDefinitionIdKey,
	string DefinitionId
) : IBlock;

[BlockHandlerRegistry]
public class ModifyDeckAddBlockHandler : IBlockHandler<ModifyDeckAddBlock>
{
	public bool Handle(IServiceContainer service, ModifyDeckAddBlock request)
	{
		if (!service.State.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }

		deck.DefinitionIds.Add(request.DeckDefinitionIdKey, request.DefinitionId);

		return true;
	}
}