using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class DetachCardFromUnitSlotBlock(
	EntityId UnitSlot,
	EntityId CardId
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
			card.OwnerPlayerId = null;
		}
	}
}