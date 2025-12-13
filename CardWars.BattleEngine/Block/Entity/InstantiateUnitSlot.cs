using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block.Entity;

public record struct InstantiateUnitSlotBlock(
	UnitSlotId UnitSlotId
) : IBlock;

public class InstantiateUnitSlotBlockHandler : IBlockHandler<InstantiateUnitSlotBlock>
{
	public bool Handle(IServiceContainer service, InstantiateUnitSlotBlock request)
	{
		if (service.State.UnitSlots.ContainsKey(request.UnitSlotId)) { return false; }
		service.State.UnitSlots[request.UnitSlotId] = new UnitSlot(request.UnitSlotId);
		return true;
	}
}