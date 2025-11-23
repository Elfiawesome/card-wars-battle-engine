using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachUnitCardToPlayerBlock(
	UnitCardId UnitCardId,
	PlayerId PlayerId,
	PlayerHandId PlayerHandId
) : IBlock;

public class AttachUnitCardToPlayerBlockHandler : IBlockHandler<AttachUnitCardToPlayerBlock>
{
	public bool Handle(BattleEngine context, AttachUnitCardToPlayerBlock request)
	{
		if (!context.EntityService.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (!context.EntityService.Players.TryGetValue(request.PlayerId, out var player)) { return false; }
		if (unitCard == null) { return false; }
		if (player == null) { return false; }

		player.HandUnitCards.Add(request.PlayerHandId, request.UnitCardId);
		return true;
	}
}