using CardWars.BattleEngine.Core.States;

namespace CardWars.BattleEngine.Core.Actions.ActionDatas;

// Create Unit
public class CreateUnitActionData(UnitId unitId) : ActionData
{
	public UnitId UnitId { get; } = unitId;
}

// Create Unit Slot
public class CreateUnitSlotActionData(UnitSlotId unitSlotId) : ActionData
{
	public UnitSlotId UnitSlotId { get; } = unitSlotId;
}

// Attaching Unit -> Unit Slot
public class AttachUnitToUnitSlotActionData(UnitId unitId, UnitSlotId unitSlotId) : ActionData
{
	public UnitId UnitId { get; } = unitId;
	public UnitSlotId UnitSlotId { get; } = unitSlotId;
}

