using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct InstantiatePlayerBlock(
	PlayerId PlayerId
) : IBlock;

[BlockHandlerRegistry]
public class InstantiatePlayerBlockHandler : IBlockHandler<InstantiatePlayerBlock>
{
	public bool Handle(IServiceContainer service, InstantiatePlayerBlock request)
	{
		if (service.State.Players.ContainsKey(request.PlayerId)) { return false; }
		service.State.Players[request.PlayerId] = new Player(request.PlayerId);
		return true;
	}
}