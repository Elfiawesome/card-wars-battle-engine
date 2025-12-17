
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record struct ModifyUnitCardHpNewBlock(
	UnitCardId UnitCardId,
	StatLayer StatLayer
) : IBlock;

public class ModifyUnitCardHpNewBlockHandler : IBlockHandler<ModifyUnitCardHpNewBlock>
{
	public bool Handle(IServiceContainer service, ModifyUnitCardHpNewBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (unitCard == null) { return false; }

		unitCard.Hp.Layers.Add(request.StatLayer);

		return true;
	}
}