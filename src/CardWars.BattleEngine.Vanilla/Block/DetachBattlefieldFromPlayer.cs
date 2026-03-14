using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachBattlefieldFromPlayerBlock(
	[property: DataTag] EntityId PlayerId,
	[property: DataTag] EntityId BattlefieldId
) : IBlock;

public class DetachBattlefieldFromPlayerHandler : IBlockHandler<DetachBattlefieldFromPlayerBlock>
{
	public void Handle(GameState context, DetachBattlefieldFromPlayerBlock request)
	{
		if (context.Require<Battlefield>(request.BattlefieldId) is not { } battlefield) return;
		if (context.Require<Player>(request.PlayerId) is not { } player) return;

		if (player.BattlefieldIds.Contains(request.BattlefieldId))
		{
			player.BattlefieldIds.Remove(request.BattlefieldId);
			battlefield.OwnerPlayerId = null;
		}
	}
}