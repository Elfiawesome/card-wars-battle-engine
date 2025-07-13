using Elfiawesome.CardWarsBattleEngine.Game;

namespace Elfiawesome.CardWarsBattleEngine;

public class CardWarsBattleEngine
{
	private readonly Dictionary<Guid, Player> _players = [];
	private readonly Dictionary<Guid, Battlefield> _battlefields = [];
	public IReadOnlyDictionary<Guid, Player> Players => _players;
	public IReadOnlyDictionary<Guid, Battlefield> Battlefields => _battlefields;
	private List<GameAction> _processingActions = [];

	public Player AddPlayer() // Put card data in here?
	{
		var newId = Guid.NewGuid();
		var player = new Player(newId);
		_players[newId] = player;
		return player;
	}

	public Battlefield AddBattlefield()
	{
		var newId = Guid.NewGuid();
		var battlefield = new Battlefield(newId);
		_battlefields[newId] = battlefield;
		return battlefield;
	}

	public void StartGame()
	{
		_processingActions.Add(new SetupGame());
	}
}

public abstract class GameAction
{

}

public class SetupGame : GameAction
{

}

public class DrawCard : GameAction
{
	
}