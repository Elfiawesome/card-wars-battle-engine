using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record ModifyDeckAddBlock(
	DeckId DeckId,
	List<string> DefinitionCardIds
) : IBlock;

public class ModifyDeckAddBlockHandler : IBlockHandler<ModifyDeckAddBlock>
{
	public bool Handle(BattleEngine context, ModifyDeckAddBlock request)
	{
		if (!context.EntityService.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (deck == null) { return false; }

		request.DefinitionCardIds.ForEach((id) =>
		{
			deck.DefinitionIds.Add(new DeckDefinitionId(Guid.NewGuid()), id);
		});
		return true;
	}
}