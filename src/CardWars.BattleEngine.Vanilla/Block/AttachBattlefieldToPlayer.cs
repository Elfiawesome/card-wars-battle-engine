using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class AttachBattlefieldToPlayerBlock(
	EntityId PlayerId,
	EntityId BattlefieldId
) : IBlock;

public class AttachBattlefieldToPlayerBlockHandler : IBlockHandler<AttachBattlefieldToPlayerBlock>
{
	public void Handle(GameState context, AttachBattlefieldToPlayerBlock request)
	{
		var player = context.Get<Player>(request.PlayerId);
		var battlefield = context.Get<Battlefield>(request.BattlefieldId);
		if (player == null || battlefield == null) { return; }

		if (player.BattlefieldIds.Contains(request.BattlefieldId)) { return; }
		player.BattlefieldIds.Add(request.BattlefieldId);
		battlefield.OwnerPlayerId = request.PlayerId;
	}
}