using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;

namespace CardWars.BattleEngine.Vanilla.Block;

public record class InstantiateUnitSlotBlock(
	EntityId Id
) : IBlock;

public class InstantiateUnitSlotBlockHandler : IBlockHandler<InstantiateUnitSlotBlock>
{
	public void Handle(GameState context, InstantiateUnitSlotBlock request)
	{
		if (context.Get(request.Id) != null) return;
		var unitSlot = new UnitSlot(request.Id);
		context.Add(unitSlot);
	}
}