using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardRemovePlayableOnBlock(
	UnitCardId UnitCardId,
	TargetPlay TargetPlay
) : IBlock;

public class ModifyUnitCardRemovePlayableOnBlockHandler : IBlockHandler<ModifyUnitCardRemovePlayableOnBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardRemovePlayableOnBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unit)) { return false; }
		if (unit == null) { return false; }

		unit.PlayableOn.Remove(request.TargetPlay);
		return true;
	}
}