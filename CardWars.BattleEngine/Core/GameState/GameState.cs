namespace CardWars.BattleEngine.Core.GameState;

public class GameState
{
	public Dictionary<PlayerStateId, PlayerState> Players = [];
	public Dictionary<BattlefieldStateId, BattlefieldState> Battlefields = [];
	public Dictionary<UnitSlotStateId, UnitSlotState> UnitSlots = [];
	public Dictionary<UnitStateId, UnitState> Units = [];
	public Dictionary<AbilityStateId, AbilityState> Abilities = [];

	public GameState()
	{
		// TODO: Remove this temporary game setup
		foreach (var i in Enumerable.Range(0, 2))
		{
			var player = new PlayerState(new PlayerStateId(Guid.NewGuid()));
			player.Name = $"Player {i}";
			var battlefield = new BattlefieldState(new BattlefieldStateId(Guid.NewGuid()));
			Players[player.Id] = player;
			Battlefields[battlefield.Id] = battlefield;
			player.ControllingBattlefields.Add(battlefield.Id);

			BattlefieldPos[] girdPos = [
				new(0,0),new(1,0),new(2,0),
				new(1,1),
			];
			foreach (var pos in girdPos)
			{
				var unitSlot = new UnitSlotState(new UnitSlotStateId(Guid.NewGuid()), pos);
				UnitSlots[unitSlot.Id] = unitSlot;
				battlefield.Grid[unitSlot.Position] = unitSlot.Id;
			}
		}
	}
}
