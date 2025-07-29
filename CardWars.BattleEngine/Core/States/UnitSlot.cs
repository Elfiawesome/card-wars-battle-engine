namespace CardWars.BattleEngine.Core.States;

public class UnitSlot(UnitSlotId id)
{
	public readonly UnitSlotId Id = id;
	public UnitId? HoldingUnit { get; set; }
	public UnitSlotPos Position { get; set; } = new(0, 0);
}

public readonly record struct UnitSlotId(long Value);
public readonly record struct UnitSlotPos(long X, long Y);

