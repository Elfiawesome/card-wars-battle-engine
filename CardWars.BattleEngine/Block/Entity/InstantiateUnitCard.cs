using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record InstantiateUnitCardBlock(
	UnitCardId UnitCardId
) : IBlock;

public class InstantiateUnitCardBlockHandler : IBlockHandler<InstantiateUnitCardBlock>
{
	public bool Handle(BattleEngine context, InstantiateUnitCardBlock request)
	{
		if (context.EntityService.UnitCards.ContainsKey(request.UnitCardId)) { return false; }
		context.EntityService.UnitCards[request.UnitCardId] = new UnitCard(context.EntityService, request.UnitCardId);
		return true;
	}
}