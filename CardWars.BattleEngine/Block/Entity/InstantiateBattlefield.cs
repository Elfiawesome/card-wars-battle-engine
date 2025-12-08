using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record InstantiateBattlefieldBlock(
	BattlefieldId BattlefieldId
) : IBlock;

public class InstantiateBattlefieldBlockHandler : IBlockHandler<InstantiateBattlefieldBlock>
{
	public bool Handle(BattleEngine context, InstantiateBattlefieldBlock request)
	{
		if (context.EntityService.Battlefields.ContainsKey(request.BattlefieldId)) { return false; }
		context.EntityService.Battlefields[request.BattlefieldId] = new Battlefield(request.BattlefieldId);
		return true;
	}
}