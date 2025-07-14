namespace Elfiawesome.CardWarsBattleEngine.Game.State;

public record class GameState
{
	public static GameState FromEngine(CardWarsBattleEngine engine)
	{
		var gs = new GameState();
		return gs;
	}

	public Dictionary<string, PlayerState> Players { get; set; } = [];
}

public record PlayerState
{

}