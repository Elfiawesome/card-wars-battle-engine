namespace CardWars.BattleEngine.Core.GameState;

public class BattlefieldState
{
	public readonly BattlefieldStateId Id;
	public Dictionary<BattlefieldPos, UnitSlotStateId> Grid = [];
	
	public BattlefieldState(BattlefieldStateId id)
	{
		Id = id;
	}
}

public readonly record struct BattlefieldStateId(Guid Value);
public readonly record struct BattlefieldPos(int X = 0, int Y = 0);