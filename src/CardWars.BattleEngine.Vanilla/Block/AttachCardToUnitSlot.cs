using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class AttachCardToUnitSlotBlock(
	EntityId UnitSlotId,
	EntityId CardId
) : IBlock;

public class AttachCardToUnitSlotBlockHandler : IBlockHandler<AttachCardToUnitSlotBlock>
{
	public void Handle(GameState context, AttachCardToUnitSlotBlock request)
	{
		var unitSlot = context.Get<UnitSlot>(request.UnitSlotId);
		var card = context.Get<GenericCard>(request.CardId);
		if (unitSlot == null || card == null) { return; }

		if (unitSlot.HoldingCardId != null) { return; }
		unitSlot.HoldingCardId = request.CardId;
		card.OwnerUnitSlotId = request.UnitSlotId;
	}
}