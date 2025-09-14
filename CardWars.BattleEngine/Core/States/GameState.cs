using CardWars.BattleEngine.Core.Events;

namespace CardWars.BattleEngine.Core.States;

public class GameState
{
	public EventManager EventManager = new();
	public Dictionary<PlayerId, PlayerState> Players = [];
	public Dictionary<BattlefieldId, BattlefieldState> Battlefields = [];
	public Dictionary<UnitId, UnitState> Units = [];
	public Dictionary<UnitSlotId, UnitSlotState> UnitSlots = [];
	public Dictionary<AbilityId, AbilityState> Abilities = [];
	public List<PlayerId> TurnOrder = [];
	public int TurnOrderIndex = 0;
	public PlayerId? CurrentPlayer
	{
		get
		{
			if (TurnOrder.Count == 0) { return null; }
			if (TurnOrderIndex >= TurnOrder.Count) { return TurnOrder[TurnOrder.Count - 1]; }
			return TurnOrder[TurnOrderIndex];
		}
	}



	public GameState()
	{
	}

	private long _counter;
	public long NewId => _counter++;
}
