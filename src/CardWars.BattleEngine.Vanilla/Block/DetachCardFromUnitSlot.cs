using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachCardFromUnitSlotBlock(
	[property: DataTag] EntityId UnitSlot,
	[property: DataTag] EntityId CardId
) : IBlock;

public class DetachCardFromUnitSlotBlockHandler : IBlockHandler<DetachCardFromUnitSlotBlock>
{
	public void Handle(GameState context, DetachCardFromUnitSlotBlock request)
	{
		var unitSlot = context.Get<UnitSlot>(request.UnitSlot);
		var card = context.Get<GenericCard>(request.CardId);
		if (unitSlot == null || card == null) { return; }

		if (unitSlot.HoldingCardId == request.CardId)
		{
			unitSlot.HoldingCardId = null;
			card.OwnerUnitSlotId = null;
		}
	}
}