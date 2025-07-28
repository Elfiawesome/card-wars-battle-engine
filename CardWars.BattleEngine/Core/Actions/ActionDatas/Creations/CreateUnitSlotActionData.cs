using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionDatas.Creations;

public class CreateUnitSlotActionData(UnitSlotId unitSlotId) : ActionData
{
	public UnitSlotId UnitSlotId { get; } = unitSlotId;
}