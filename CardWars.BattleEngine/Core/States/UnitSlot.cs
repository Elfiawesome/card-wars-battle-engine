namespace CardWars.BattleEngine.Core.States;

public class UnitSlotState(GameState gameState, UnitSlotId id) : BaseState<UnitSlotId>(gameState, id)
{
	public UnitId? HoldingUnit;
}
public readonly record struct UnitSlotId(long Value) : IBaseId;
