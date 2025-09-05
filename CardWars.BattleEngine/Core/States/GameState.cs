using CardWars.BattleEngine.Core.EventSystem;

namespace CardWars.BattleEngine.Core.States;

public class GameState
{
	public EventManager EventManager = new();
	public Dictionary<PlayerId, PlayerState> Players = [];
	public Dictionary<BattlefieldId, BattlefieldState> Battlefields = [];
	public Dictionary<UnitId, UnitState> Units = [];
	public Dictionary<UnitSlotId, UnitSlotState> UnitSlots = [];
	public Dictionary<AbilityId, AbilityState> Abilities = [];


	public GameState()
	{
	}

	private long _counter;
	public long NewId => _counter++;
}
