namespace CardWars.BattleEngine.Core.States;

public class BattlefieldState(GameState gameState, BattlefieldId id) : BaseState<BattlefieldId>(gameState, id)
{
	public List<UnitSlotId> UnitSlots = [];
}
public readonly record struct BattlefieldId(long Value) : IBaseId;
