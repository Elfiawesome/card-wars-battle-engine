using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachBattlefieldToPlayerBlock(
	BattlefieldId BattlefieldId,
	PlayerId PlayerId
) : IBlock;

public class AttachBattlefieldToPlayerBlockHandler : IBlockHandler<AttachBattlefieldToPlayerBlock>
{
	public bool Handle(BattleEngine context, AttachBattlefieldToPlayerBlock request)
	{
		if (context.EntityService.Battlefields.TryGetValue(request.BattlefieldId, out var battlefield)) { return false; }
		if (context.EntityService.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (battlefield == null) { return false; }
		if (player == null) { return false; }
		
		if (player.ControllingBattlefieldIds.Contains(request.BattlefieldId)) { return false; }
		battlefield.OwnerPlayerId = request.PlayerId;
		player.ControllingBattlefieldIds.Add(request.BattlefieldId);
		return true;
	}
}