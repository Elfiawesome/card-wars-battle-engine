using CardWars.BattleEngine.Entity;
using CardWars.BattleEngine.Input;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachUnitSlotToBattlefieldBlock(
	UnitSlotId UnitSlotId,
	BattlefieldId BattlefieldId
) : IBlock;

public class AttachUnitSlotToBattlefieldBlockHandler : IBlockHandler<AttachUnitSlotToBattlefieldBlock>
{
	public bool Handle(BattleEngine context, AttachUnitSlotToBattlefieldBlock request)
	{
		if (!context.EntityService.UnitSlots.TryGetValue(request.UnitSlotId, out var unitSlot)) { return false; }
		if (!context.EntityService.Battlefields.TryGetValue(request.BattlefieldId, out var battlefield)) { return false; }
		if (unitSlot == null) { return false; }
		if (battlefield == null) { return false; }
		
		if (battlefield.UnitSlotIds.Contains(request.UnitSlotId)) { return false; }
		unitSlot.OwnerBattlefieldId = request.BattlefieldId;
		battlefield.UnitSlotIds.Add(request.UnitSlotId);
		return true;
	}
}