namespace Elfiawesome.CardWarsBattleEngine.Game.Entities;

public class UnitSlot
{
	public Unit? Unit;
	public readonly UnitSlotPos Position;

	public UnitSlot(UnitSlotPos position)
	{
		Position = position;
	}
}

public readonly record struct UnitSlotPos(int X, int Y);