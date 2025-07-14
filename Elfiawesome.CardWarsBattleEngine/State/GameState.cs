namespace Elfiawesome.CardWarsBattleEngine.State;

public record class GameState
{
	public static GameState FromEngine(CardWarsBattleEngine engine)
	{
		var gs = new GameState();
		foreach (var p in engine.Players)
		{
			gs.Players[p.Value.Id.Id] = new PlayerState(p.Value.Id.Id, p.Value.Name);
		}

		foreach (var b in engine.Battlefields)
		{
			gs.Battlefields[b.Value.Id.Id] = new BattlefieldState(b.Value.Id.Id);
		}

		return gs;
	}

	public Dictionary<long, PlayerState> Players { get; set; } = [];
	public Dictionary<long, BattlefieldState> Battlefields { get; set; } = [];
}

public record PlayerState(long Id, string Name)
{
	public long Id { get; set; } = Id;
	public string Name { get; set; } = Name;
}

public record BattlefieldState(long Id)
{
	public long Id { get; set; } = Id;
}