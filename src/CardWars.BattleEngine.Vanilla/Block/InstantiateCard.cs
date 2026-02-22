using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class InstantiateCardBlock(
	EntityId Id
) : IBlock;

public class InstantiateCardBlockHandler : IBlockHandler<InstantiateCardBlock>
{
	public void Handle(GameState context, InstantiateCardBlock request)
	{
		if (context.Get(request.Id) != null) return;
		context.Add(new GenericCard(request.Id));
	}
}