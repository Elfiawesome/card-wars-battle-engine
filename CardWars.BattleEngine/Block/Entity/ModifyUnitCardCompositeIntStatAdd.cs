using CardWars.BattleEngine.State.Component;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardCompositeIntStatAddBlock(
	UnitCardId UnitCardId,
	string StatName,
	StatLayer StatLayer
) : IBlock;

public class ModifyUnitCardCompositeIntStatAddBlockHandler : IBlockHandler<ModifyUnitCardCompositeIntStatAddBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardCompositeIntStatAddBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unit)) { return false; }
		if (unit == null) { return false; }

		var compositeIntStat = service.Mapping.GetValue<CompositeIntStat>(unit, request.StatName);
		if (compositeIntStat == null) { return false; }

		compositeIntStat.Layers.Add(request.StatLayer);
		return true;
	}
}