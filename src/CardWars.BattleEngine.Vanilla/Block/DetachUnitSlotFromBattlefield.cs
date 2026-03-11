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
		var battlefield = context.Get<Battlefield>(request.BattlefieldId);
		var unitSlot = context.Get<UnitSlot>(request.UnitSlotId);
		if (battlefield == null || unitSlot == null) { return; }

		if (battlefield.UnitSlotIds.Contains(request.UnitSlotId))
		{
			battlefield.UnitSlotIds.Remove(request.UnitSlotId);
			unitSlot.OwnerBattlefieldId = Guid.Empty; // TODO : Idk if i should make it EntityId?
		}
	}
}