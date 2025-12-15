using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachUnitCardToPlayerBlock(
	UnitCardId UnitCardId,
	PlayerId PlayerId,
	PlayerHandId PlayerHandId
) : IBlock;

public class AttachUnitCardToPlayerBlockHandler : IBlockHandler<AttachUnitCardToPlayerBlock>
{
	public bool Handle(IServiceContainer service, AttachUnitCardToPlayerBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (!service.State.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (unitCard == null) { return false; }
		if (player == null) { return false; }

		if (player.HandUnitCards.ContainsKey(request.PlayerHandId)) { return false; }
		if (player.HandOrder.Contains(request.PlayerHandId)) { return false; }
		
		player.HandUnitCards.Add(request.PlayerHandId, request.UnitCardId);
		player.HandOrder.Add(request.PlayerHandId);

		return true;
	}
}