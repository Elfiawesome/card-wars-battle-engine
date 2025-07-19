using System.Text.Json;

namespace CardWars.BattleEngine.Core.GameState;


// I hate this object. Need a better way to manage sending out game states
public class GameStateJsonizer
{
	public static string SerializedValue(GameState state)
	{
		return JsonSerializer.Serialize(new JsonGameState(state));
	}

	private class JsonPlayerState(PlayerState state)
	{
		public string Id { get; set; } = state.Id.Value.ToString();
		public string Name { get; set; } = state.Name;
		public List<string> ControllingBattlefields { get; set; } = state.ControllingBattlefields.Select((i) => i.Value.ToString()).ToList();
	}
	private class JsonBattlefieldState(BattlefieldState state)
	{
		public string Id { get; set; } = state.Id.Value.ToString();
		public Dictionary<string, string> Grid { get; set; } = state.Grid.ToDictionary(
			pair => pair.Key.ToString(),
			pair => pair.Value.Value.ToString()
		);
	}
	private class JsonUnitSlotState(UnitSlotState state)
	{
		public string Id { get; set; } = state.Id.Value.ToString();
		public string Unit { get; set; } = state.Unit?.Value.ToString() ?? "";
		public string Position { get; set; } = state.Position.ToString();
	}
	private class JsonUnitState(UnitState state)
	{
		public string Id { get; set; } = state.Id.Value.ToString();
	}
	private class JsonGameState
	{
		public Dictionary<string, JsonPlayerState> Players { get; set; } = [];
		public Dictionary<string, JsonBattlefieldState> Battlefields { get; set; } = [];
		public Dictionary<string, JsonUnitSlotState> UnitSlots { get; set; } = [];
		public Dictionary<string, JsonUnitState> Units { get; set; } = [];

		public JsonGameState(GameState state)
		{
			state.Players.ToList().ForEach((i) =>
			{
				Players[i.Key.Value.ToString()] = new JsonPlayerState(i.Value);
			});
			state.Battlefields.ToList().ForEach((i) =>
			{
				Battlefields[i.Key.Value.ToString()] = new JsonBattlefieldState(i.Value);
			});
			state.UnitSlots.ToList().ForEach((i) =>
			{
				UnitSlots[i.Key.Value.ToString()] = new JsonUnitSlotState(i.Value);
			});
			state.Units.ToList().ForEach((i) =>
			{
				Units[i.Key.Value.ToString()] = new JsonUnitState(i.Value);
			});
			// state.Abilities.ToList().ForEach((i) =>
			// {

			// });
		}
	}
}

