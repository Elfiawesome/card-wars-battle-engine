using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class InstantiateUnitSlotBlock(
	[property: DataTag] EntityId Id
) : IBlock;

public class InstantiateUnitSlotBlockHandler : IBlockHandler<InstantiateUnitSlotBlock>
{
	public void Handle(GameState context, InstantiateUnitSlotBlock request)
	{
		if (context.Get(request.Id) != null)
		{
			Logger.Warn($"Entity [{request.Id}] already exists, skipping InstantiateUnitSlotBlock"); return;
		}
		var unitSlot = new UnitSlot(request.Id);
		context.Add(unitSlot);
	}
}