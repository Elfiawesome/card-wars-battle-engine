namespace Elfiawesome.CardWarsBattleEngine.Game.Entities;

public class UnitSlot : Entity<UnitSlotPos>
{
	public Unit? Unit;

	public UnitSlot(UnitSlotPos id) : base(id)
	{
		
	}
}

public readonly record struct UnitSlotPos(int X, int Y);