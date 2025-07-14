namespace Elfiawesome.CardWarsBattleEngine.Game.Entities;

public class Unit : Entity<UnitId>
{
	public string Name = "";
	public int Hp = 0;
	public int Atk = 0;
	public int Pt = 0;

	public Unit(UnitId id) : base(id)
	{
	}
}

public readonly record struct UnitId(long Id);