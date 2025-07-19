namespace CardWars.BattleEngine.Core.States;

public class Unit(UnitId id)
{
	public readonly UnitId Id = id;
}

public readonly record struct UnitId(long Value);