using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block.Entity;

public record AttachUnitCardToUnitSlotBlock(
	UnitCardId UnitCardId,
	UnitSlotId UnitSlotId
) : IBlock;

public class AttachUnitCardToUnitSlotBlockHandler : IBlockHandler<AttachUnitCardToUnitSlotBlock>
{
	public bool Handle(IServiceContainer service, AttachUnitCardToUnitSlotBlock request)
	{
		if (!service.State.UnitCards.TryGetValue(request.UnitCardId, out var unitCard)) { return false; }
		if (!service.State.UnitSlots.TryGetValue(request.UnitSlotId, out var unitSlot)) { return false; }
		if (unitCard == null) { return false; }
		if (unitSlot == null) { return false; }

		unitSlot.HoldingCardId = unitCard.Id;
		unitCard.OwnerUnitSlotId = unitSlot.Id;

		return true;
	}
}