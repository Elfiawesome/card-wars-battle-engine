using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachCardToUnitSlotBlock(
	[property: DataTag] EntityId UnitSlotId,
	[property: DataTag] EntityId CardId
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