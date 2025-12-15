using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachUnitCardToPlayerBlock(
	UnitCardId UnitCardId,
	PlayerId PlayerId
) : IBlock;

public class AttachUnitCardToPlayerBlockHandler : IBlockHandler<AttachUnitCardToPlayerBlock>
{
	public bool Handle(IServiceContainer service, AttachUnitCardToPlayerBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (!service.State.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (unitCard == null) { return false; }
		if (player == null) { return false; }

		if (player.HandCards.Contains(request.UnitCardId)) { return false; }
		player.HandCards.Add(request.UnitCardId);

		return true;
	}
}