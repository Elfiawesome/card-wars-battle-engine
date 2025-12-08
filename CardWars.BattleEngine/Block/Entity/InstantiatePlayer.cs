using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record InstantiatePlayerBlock(
	PlayerId PlayerId
) : IBlock;

public class InstantiatePlayerBlockHandler : IBlockHandler<InstantiatePlayerBlock>
{
	public bool Handle(BattleEngine context, InstantiatePlayerBlock request)
	{
		if (context.EntityService.Players.ContainsKey(request.PlayerId)) { return false; }
		context.EntityService.Players[request.PlayerId] = new Player(request.PlayerId);
		return true;
	}
}