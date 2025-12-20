using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct InstantiateBattlefieldBlock(
	BattlefieldId BattlefieldId
) : IBlock;

public class InstantiateBattlefieldBlockHandler : IBlockHandler<InstantiateBattlefieldBlock>
{
	public bool Handle(IServiceContainer service, InstantiateBattlefieldBlock request)
	{
		if (service.State.Battlefields.ContainsKey(request.BattlefieldId)) { return false; }
		service.State.Battlefields[request.BattlefieldId] = new Battlefield(request.BattlefieldId);
		return true;
	}
}