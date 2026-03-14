using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachDeckToPlayerBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId DeckId
) : IBlock;

public class AttachDeckToPlayerBlockHandler : IBlockHandler<AttachDeckToPlayerBlock>
{
	public void Handle(GameState context, AttachDeckToPlayerBlock request)
	{
		if (context.Require<Player>(request.PlayerId) is not { } player) return;
		if (context.Require<Deck>(request.DeckId) is not { } deck) return;
		
		if (player.DeckIds.Contains(request.DeckId)) { return; }
		deck.OwnerPlayerId = request.PlayerId;
		player.DeckIds.Add(request.DeckId);
	}
}