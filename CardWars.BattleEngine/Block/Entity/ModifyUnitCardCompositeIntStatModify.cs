using CardWars.BattleEngine.State.Component;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardCompositeIntStatSetBlock(
	UnitCardId UnitCardId,
	string StatName,
	StatLayer NewStatLayer
) : IBlock;

public class ModifyUnitCardCompositeIntStatSetBlockHandler : IBlockHandler<ModifyUnitCardCompositeIntStatSetBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardCompositeIntStatSetBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unit)) { return false; }
		if (unit == null) { return false; }

		var compositeIntStat = service.Mapping.GetValue<CompositeIntStat>(unit, request.StatName);
		if (compositeIntStat == null) { return false; }

		compositeIntStat.SetLayer(request.NewStatLayer);
		
		return true;
	}
}