using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachCardFromUnitSlotBlock(
	[property: DataTag] EntityId UnitSlotId,
	[property: DataTag] EntityId CardId
) : IBlock;

public class DetachCardFromUnitSlotBlockHandler : IBlockHandler<DetachCardFromUnitSlotBlock>
{
	public void Handle(GameState context, DetachCardFromUnitSlotBlock request)
	{
		if (context.Require<UnitSlot>(request.UnitSlotId) is not { } unitSlot) return;
		if (context.Require<GenericCard>(request.CardId) is not { } card) return;

		if (unitSlot.HoldingCardId == request.CardId)
		{
			unitSlot.HoldingCardId = null;
			card.OwnerUnitSlotId = null;
		}
	}
}