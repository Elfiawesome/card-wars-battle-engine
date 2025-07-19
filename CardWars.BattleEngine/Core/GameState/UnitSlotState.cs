namespace CardWars.BattleEngine.Core.GameState;

public class UnitSlotState
{
	public readonly UnitSlotStateId Id;
	public readonly BattlefieldPos Position;
	public UnitStateId? Unit;

	public UnitSlotState(UnitSlotStateId id, BattlefieldPos position)
	{
		Id = id;
		Position = position;
	}
}

public readonly record struct UnitSlotStateId(Guid Value);