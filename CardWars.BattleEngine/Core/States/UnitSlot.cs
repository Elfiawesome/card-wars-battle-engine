namespace CardWars.BattleEngine.Core.States;

public class UnitSlot(UnitSlotId id)
{
	public readonly UnitSlotId Id = id;
}

public readonly record struct UnitSlotId(long Value);