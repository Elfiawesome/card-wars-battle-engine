using Elfiawesome.CardWarsBattleEngine.Game.Battlefields;
using Elfiawesome.CardWarsBattleEngine.Game.Intents;
using Elfiawesome.CardWarsBattleEngine.Game.Players;

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
	// private List<GameAction> _ActionQueue = [];
	private PlayerIdGenerator _playerIdGenerator = new();
	private BattlefieldIdGenerator _battlefieldIdGenerator = new();
	

	public Player AddPlayer() // Put card data in here?
	{
		var newId = _playerIdGenerator.NewId();
		var player = new Player(newId);
		_players[newId] = player;
		return player;
	}

	public Battlefield AddBattlefield()
	{
		var newId = _battlefieldIdGenerator.NewId();
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