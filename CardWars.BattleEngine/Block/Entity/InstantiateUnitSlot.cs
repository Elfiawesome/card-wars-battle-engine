using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record InstantiateUnitSlotBlock(
	UnitSlotId UnitSlotId
) : IBlock;

public class InstantiateUnitSlotBlockHandler : IBlockHandler<InstantiateUnitSlotBlock>
{
	public bool Handle(BattleEngine context, InstantiateUnitSlotBlock request)
	{
		if (context.EntityService.UnitSlots.ContainsKey(request.UnitSlotId)) { return false; }
		context.EntityService.UnitSlots[request.UnitSlotId] = new UnitSlot(request.UnitSlotId);
		return true;
	}
}