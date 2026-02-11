using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class InstantiateDeckBlock(
	EntityId Id
) : IBlock;

public class InstantiateDeckBlockHandler : IBlockHandler<InstantiateDeckBlock>
{
	public void Handle(GameState context, InstantiateDeckBlock request)
	{
		if (context.Get(request.Id) != null) return;
		context.Add(new Deck(request.Id));
	}
}