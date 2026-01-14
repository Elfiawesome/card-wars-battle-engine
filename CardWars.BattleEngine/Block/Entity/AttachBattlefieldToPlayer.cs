using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachBattlefieldToPlayerBlock(
	BattlefieldId BattlefieldId,
	PlayerId PlayerId
) : IBlock;

[BlockHandlerRegistry]
public class AttachBattlefieldToPlayerBlockHandler : IBlockHandler<AttachBattlefieldToPlayerBlock>
{
	public bool Handle(IServiceContainer service, AttachBattlefieldToPlayerBlock request)
	{
		if (!service.State.Battlefields.TryGetValue(request.BattlefieldId, out var battlefield)) { return false; }
		if (!service.State.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (battlefield == null) { return false; }
		if (player == null) { return false; }

		if (player.ControllingBattlefieldIds.Contains(request.BattlefieldId)) { return false; }
		battlefield.OwnerPlayerId = request.PlayerId;
		player.ControllingBattlefieldIds.Add(request.BattlefieldId);
		return true;
	}
}