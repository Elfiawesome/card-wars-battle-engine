using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class DetachUnitSlotFromBattlefieldBlock(
	[property: DataTag] EntityId BattlefieldId,
	[property: DataTag] EntityId UnitSlotId
) : IBlock;

public class DetachUnitSlotFromBattlefieldBlockHandler : IBlockHandler<DetachUnitSlotFromBattlefieldBlock>
{
	public void Handle(GameState context, DetachUnitSlotFromBattlefieldBlock request)
	{
		if (context.Require<Battlefield>(request.BattlefieldId) is not { } battlefield) return;
		if (context.Require<UnitSlot>(request.UnitSlotId) is not { } unitSlot) return;

		if (battlefield.UnitSlotIds.Contains(request.UnitSlotId))
		{
			battlefield.UnitSlotIds.Remove(request.UnitSlotId);
			unitSlot.OwnerBattlefieldId = null;
		}
	}
}