namespace CardWars.BattleEngine.Core.States;

public class UnitState : BaseState<UnitId>
{
	public string Name = "";
	public int Hp = 0;
	public int Atk = 0;
	public int Pt = 0;

	public HashSet<AbilityId> Abilities = [];

	public UnitState(GameState gameState, UnitId id) : base(gameState, id)
	{

	}

}
public readonly record struct UnitId(long Value) : IBaseId;