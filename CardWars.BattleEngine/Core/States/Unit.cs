namespace CardWars.BattleEngine.Core.States;

public class Unit(UnitId id)
{
	public readonly UnitId Id = id;
	public UnitSlotId? ParentUnitSlotId { get; set; }
	public string Name { get; set; } = "";
	public int Hp { get; set; } = 10;
	public int Atk { get; set; } = 10;
	public int Pt { get; set; } = 10;
}

public readonly record struct UnitId(long Value);