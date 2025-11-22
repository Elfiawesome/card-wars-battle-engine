using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachDeckToPlayerBlock(
	DeckId DeckId,
	PlayerId PlayerId,
	bool IsUnitDeck = false,
	bool IsSpellDeck = false
) : IBlock;

public class AttachDeckToPlayerBlockHandler : IBlockHandler<AttachDeckToPlayerBlock>
{
	public bool Handle(BattleEngine context, AttachDeckToPlayerBlock request)
	{
		if (!context.EntityService.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (!context.EntityService.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (deck == null) { return false; }
		if (player == null) { return false; }

		if (request.IsUnitDeck)
		{
			player.UnitDeckId = request.DeckId;
		}
		if (request.IsSpellDeck)
		{
			player.SpellDeckId = request.DeckId;		
		}
		return true;
	}
}