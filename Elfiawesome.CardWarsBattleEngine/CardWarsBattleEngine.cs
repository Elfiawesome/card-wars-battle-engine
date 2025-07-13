using System.Numerics;
using Elfiawesome.CardWarsBattleEngine.Game;

namespace Elfiawesome.CardWarsBattleEngine;

public class CardWarsBattleEngine
{
	private readonly Dictionary<Guid, Player> _players = [];
	private readonly Dictionary<Guid, Battlefield> _battlefields = [];
	public IReadOnlyDictionary<Guid, Player> Players => _players;
	public IReadOnlyDictionary<Guid, Battlefield> Battlefields => _battlefields;

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
		var battlefield = new Battlefield(newId, 3, 1);
		_battlefields[newId] = battlefield;
		return battlefield;
	}
}
