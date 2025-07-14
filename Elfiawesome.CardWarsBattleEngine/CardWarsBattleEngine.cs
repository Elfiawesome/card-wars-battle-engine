using Elfiawesome.CardWarsBattleEngine.Game;
using Elfiawesome.CardWarsBattleEngine.Game.Intents;

namespace Elfiawesome.CardWarsBattleEngine;

public class CardWarsBattleEngine
{
	// Public APIs
	public IReadOnlyDictionary<Guid, Player> Players => _players;
	public IReadOnlyDictionary<Guid, Battlefield> Battlefields => _battlefields;

	// Internal Apis
	internal readonly Dictionary<Guid, Player> _players = [];
	internal readonly Dictionary<Guid, Battlefield> _battlefields = [];
	internal bool isGameStarted = false;

	// Self-contained own usage
	private List<GameIntent> _intentQueue = [];
	

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

	private void QueueIntent(GameIntent intent)
	{
		intent.IntentCompletedEvent += () => OnIntentCompeted(intent);
		_intentQueue.Add(intent);
		ProcessIntentQueue();
	}
}