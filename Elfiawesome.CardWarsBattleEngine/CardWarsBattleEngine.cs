using Elfiawesome.CardWarsBattleEngine.Game.Actions;
using Elfiawesome.CardWarsBattleEngine.Game.Entities;
using Elfiawesome.CardWarsBattleEngine.Game.Intents;

namespace Elfiawesome.CardWarsBattleEngine;

public class CardWarsBattleEngine
{
	// Public APIs
	public IReadOnlyDictionary<PlayerId, Player> Players => _players;
	public IReadOnlyDictionary<BattlefieldId, Battlefield> Battlefields => _battlefields;

	// Internal Apis
	internal readonly Dictionary<PlayerId, Player> _players = [];
	internal readonly Dictionary<BattlefieldId, Battlefield> _battlefields = [];
	internal bool isGameStarted = false;

	// Self-contained own usage
	private List<GameIntent> _intentQueue = [];
	private long _playerIdCounter;
	private long _battlefieldIdCounter;

	public void QueueIntent(GameIntent intent)
	{
		intent.IntentCompletedEvent += () => OnIntentCompeted(intent);
		_intentQueue.Add(intent);
		ProcessIntentQueue();
	}

	public void QueueAction(GameAction action)
	{
		// For now we just initiate the GameAction immediately
		// Note that GameActions are the bare building blocks of the game
		// Game Actions can only change the state of the game and not make more actions
		action.Execute(this);
	}

	public Player AddPlayer() // Put card data in here?
	{
		var newId = new PlayerId(_playerIdCounter++);
		var player = new Player(newId);
		_players[newId] = player;
		return player;
	}

	public Battlefield AddBattlefield()
	{
		var newId = new BattlefieldId(_battlefieldIdCounter++);
		var battlefield = new Battlefield(newId);
		_battlefields[newId] = battlefield;
		return battlefield;
	}

	public void StartGame()
	{
		QueueIntent(new SetupGame());
	}

	private void ProcessIntentQueue()
	{
		if (_intentQueue.Count > 0)
		{
			var intent = _intentQueue[0];
			intent.Execute(this);
		}
	}

	private void OnIntentCompeted(GameIntent intent)
	{
		_intentQueue.Remove(intent);
		ProcessIntentQueue();
	}
}