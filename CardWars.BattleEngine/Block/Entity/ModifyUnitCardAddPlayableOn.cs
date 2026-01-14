using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardAddPlayableOnBlock(
	UnitCardId UnitCardId,
	TargetPlay TargetPlay
) : IBlock;

[BlockHandlerRegistry]
public class ModifyUnitCardAddPlayableOnBlockHandler : IBlockHandler<ModifyUnitCardAddPlayableOnBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardAddPlayableOnBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unit)) { return false; }
		if (unit == null) { return false; }

		unit.PlayableOn.Add(request.TargetPlay, []);
		return true;
	}
}