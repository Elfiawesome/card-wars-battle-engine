using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record DetachUnitCardFromPlayerBlock(
	UnitCardId UnitCardId,
	PlayerId PlayerId
) : IBlock;

public class DetachUnitCardFromPlayerBlockHandler : IBlockHandler<DetachUnitCardFromPlayerBlock>
{
	public bool Handle(IServiceContainer service, DetachUnitCardFromPlayerBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (!service.State.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (unitCard == null) { return false; }
		if (player == null) { return false; }

		if (!player.HandCards.Contains(request.UnitCardId)) { return false; }
		player.HandCards.Remove(request.UnitCardId);
		unitCard.OwnerPlayerId = null;

		return true;
	}
}