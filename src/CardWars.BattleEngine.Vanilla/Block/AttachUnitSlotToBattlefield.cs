using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Block;

[DataTagType()]
public record class AttachUnitSlotToBattlefieldBlock(
	[property: DataTag] EntityId BattlefieldId,
	[property: DataTag] EntityId UnitSlotId
) : IBlock;

public class AttachUnitSlotToBattlefieldBlockHandler : IBlockHandler<AttachUnitSlotToBattlefieldBlock>
{
	public void Handle(GameState context, AttachUnitSlotToBattlefieldBlock request)
	{
		var battlefield = context.Get<Battlefield>(request.BattlefieldId);
		var unitSlot = context.Get<UnitSlot>(request.UnitSlotId);
		if (battlefield == null || unitSlot == null) { return; }

		if (battlefield.UnitSlotIds.Contains(request.UnitSlotId)) { return; }
		battlefield.UnitSlotIds.Add(request.UnitSlotId);
		unitSlot.OwnerBattlefieldId = request.BattlefieldId;
	}
}