namespace CardWars.BattleEngine.Core.GameState;

public class AbilityState
{
	public readonly AbilityStateId Id;
	public UnitStateId? ParentUnitId;

	public AbilityState(AbilityStateId id)
	{
		Id = id;
	}
}

public readonly record struct AbilityStateId(Guid Value);