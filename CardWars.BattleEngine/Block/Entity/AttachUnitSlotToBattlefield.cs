using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachUnitSlotToBattlefieldBlock(
	UnitSlotId UnitSlotId,
	BattlefieldId BattlefieldId
) : IBlock;

[BlockHandlerRegistry]
public class AttachUnitSlotToBattlefieldBlockHandler : IBlockHandler<AttachUnitSlotToBattlefieldBlock>
{
	public bool Handle(IServiceContainer service, AttachUnitSlotToBattlefieldBlock request)
	{
		if (!service.State.UnitSlots.TryGetValue(request.UnitSlotId, out var unitSlot)) { return false; }
		if (!service.State.Battlefields.TryGetValue(request.BattlefieldId, out var battlefield)) { return false; }
		if (unitSlot == null) { return false; }
		if (battlefield == null) { return false; }

		if (battlefield.ControllingUnitSlotIds.Contains(request.UnitSlotId)) { return false; }
		unitSlot.OwnerBattlefieldId = request.BattlefieldId;
		battlefield.ControllingUnitSlotIds.Add(request.UnitSlotId);
		return true;
	}
}