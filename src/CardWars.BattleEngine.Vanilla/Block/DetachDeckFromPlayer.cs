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
		var deck = context.Get<Deck>(request.DeckId);
		var player = context.Get<Player>(request.PlayerId);
		if (deck == null || player == null) { return; }

		if (player.DeckIds.Contains(request.DeckId))
		{
			player.DeckIds.Remove(request.DeckId);
			deck.OwnerPlayerId = Guid.Empty; // TODO : Idk if i should make it EntityId?
		}
	}
}