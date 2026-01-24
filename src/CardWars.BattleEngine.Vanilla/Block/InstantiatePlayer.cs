using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class InstantiatePlayerBlock(
	EntityId Id
) : IBlock;

public class InstantiatePlayerBlockHandler : IBlockHandler<InstantiatePlayerBlock>
{
	public void Handle(GameState context, InstantiatePlayerBlock request)
	{
		if (context.Get(request.Id) != null) return;
		context.Add(new Player(request.Id));
	}
}