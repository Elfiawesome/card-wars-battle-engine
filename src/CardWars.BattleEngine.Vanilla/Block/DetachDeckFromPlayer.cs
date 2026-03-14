using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachDeckFromPlayerBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId DeckId
) : IBlock;

public class DetachDeckFromPlayerBlockHandler : IBlockHandler<DetachDeckFromPlayerBlock>
{
	public void Handle(GameState context, DetachDeckFromPlayerBlock request)
	{
		if (context.Require<Deck>(request.DeckId) is not { } deck) return;
		if (context.Require<Player>(request.PlayerId) is not { } player) return;

		if (player.DeckIds.Contains(request.DeckId))
		{
			player.DeckIds.Remove(request.DeckId);
			deck.OwnerPlayerId = null;
		}
	}
}