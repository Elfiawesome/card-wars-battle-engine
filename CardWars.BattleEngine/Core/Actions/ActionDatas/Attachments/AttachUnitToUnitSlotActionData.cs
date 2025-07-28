using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionDatas.Attachments;

public class AttachUnitToUnitSlotActionData(UnitId unitId, UnitSlotId unitSlotId) : ActionData
{
	public UnitId UnitId { get; } = unitId;
	public UnitSlotId UnitSlotId { get; } = unitSlotId;
}
