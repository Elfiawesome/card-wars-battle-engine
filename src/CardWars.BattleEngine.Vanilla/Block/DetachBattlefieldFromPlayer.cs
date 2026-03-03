using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class DetachBattlefieldFromPlayerBlock(
	EntityId PlayerId,
	EntityId BattlefieldId
) : IBlock;

public class DetachBattlefieldFromPlayerHandler : IBlockHandler<DetachBattlefieldFromPlayerBlock>
{
	public void Handle(GameState context, DetachBattlefieldFromPlayerBlock request)
	{
		var battlefield = context.Get<Battlefield>(request.BattlefieldId);
		var player = context.Get<Player>(request.PlayerId);
		if (battlefield == null || player == null) { return; }

		if (player.BattlefieldIds.Contains(request.BattlefieldId))
		{
			player.BattlefieldIds.Remove(request.BattlefieldId);
			battlefield.OwnerPlayerId = Guid.Empty; // TODO : Idk if i should make it EntityId?
		}
	}
}