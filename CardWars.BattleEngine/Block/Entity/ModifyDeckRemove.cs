using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record ModifyDeckRemoveBlock(
	DeckId DeckId,
	List<DeckDefinitionId> CardGuid,
	bool DrawTop = false
) : IBlock;

public class ModifyDeckRemoveBlockHandler : IBlockHandler<ModifyDeckRemoveBlock>
{
	public bool Handle(BattleEngine context, ModifyDeckRemoveBlock request)
	{
		if (!context.EntityService.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }
		
		request.CardGuid.ForEach((id) =>
		{
			deck.DefinitionIds.Remove(id);
		});
		return true;
	}
}