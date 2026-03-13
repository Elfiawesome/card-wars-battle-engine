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
		var player = context.Get<Player>(request.PlayerId);
		var deck = context.Get<Deck>(request.DeckId);
		if (player == null || deck == null) { return; }
		
		if (player.DeckIds.Contains(request.DeckId)) { return; }
		deck.OwnerPlayerId = request.PlayerId;
		player.DeckIds.Add(request.DeckId);
	}
}