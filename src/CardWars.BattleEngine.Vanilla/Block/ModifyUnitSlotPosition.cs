using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class ModifyUnitSlotPositionBlock(
	[property: DataTag] EntityId UnitSlotId,
	[property: DataTag] UnitSlotPos Position
) : IBlock;

public class ModifyUnitSlotPositionBlockHandler : IBlockHandler<ModifyUnitSlotPositionBlock>
{
	public void Handle(GameState context, ModifyUnitSlotPositionBlock request)
	{
		if (context.Get(request.UnitSlotId) is not UnitSlot unitSlot) return;
		unitSlot.Position = request.Position;
	}
}
