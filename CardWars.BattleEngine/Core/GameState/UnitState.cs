namespace CardWars.BattleEngine.Core.GameState;

public class UnitState
{
	public readonly UnitStateId Id;
	public List<AbilityStateId> Abilities = [];
	public string Name = "";
	public int Hp = 0;
	public int Atk = 0;
	public int Pt = 0;
	public int Charge = 0;

	public UnitState(UnitStateId id)
	{
		Id = id;
	}
}

public readonly record struct UnitStateId(Guid Value);