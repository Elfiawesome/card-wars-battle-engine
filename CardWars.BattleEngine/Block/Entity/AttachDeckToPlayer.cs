using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachDeckToPlayerBlock(
	DeckId DeckId,
	PlayerId PlayerId,
	DeckType DeckType
) : IBlock;

public class AttachDeckToPlayerBlockHandler : IBlockHandler<AttachDeckToPlayerBlock>
{
	public bool Handle(IServiceContainer service, AttachDeckToPlayerBlock request)
	{
		if (!service.State.Decks.TryGetValue(request.DeckId, out var deck)) { return false; }
		if (!service.State.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (deck == null) { return false; }
		if (player == null) { return false; }

		if (!player.ControllingDecks.ContainsKey(deck.DeckType)) { player.ControllingDecks[deck.DeckType] = []; }
		var controllingDecks = player.ControllingDecks[deck.DeckType];
		
		if (controllingDecks.Contains(request.DeckId)) { return false; }
		controllingDecks.Add(request.DeckId);
		deck.OwnerPlayerId = request.PlayerId;
		return true;
	}
}