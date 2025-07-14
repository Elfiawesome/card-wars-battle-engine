using Elfiawesome.CardWarsBattleEngine.Game.Actions;
using Elfiawesome.CardWarsBattleEngine.Game.Entities;
using Elfiawesome.CardWarsBattleEngine.Game.Intents;
using Elfiawesome.CardWarsBattleEngine.PlayerInputs;
using Elfiawesome.CardWarsBattleEngine.State;

namespace Elfiawesome.CardWarsBattleEngine;

public class CardWarsBattleEngine
{
	// Public APIs
	public Action<(GameAction, bool, GameState)>? ActionProcessed;
	public IReadOnlyDictionary<PlayerId, Player> Players => _players;
	public IReadOnlyDictionary<BattlefieldId, Battlefield> Battlefields => _battlefields;
	public PlayerId? CurrentPlayerIdTurn => (_currentTurnOrder >= 0 && _currentTurnOrder < _turnOrder.Count) ? _turnOrder[_currentTurnOrder] : null;

	// Internal Apis
	internal readonly Dictionary<PlayerId, Player> _players = [];
	internal readonly Dictionary<BattlefieldId, Battlefield> _battlefields = [];
	internal readonly Dictionary<UnitId, Unit> _units = [];
	internal bool isGameStarted = false;
	internal List<PlayerId> _turnOrder = [];
	internal int _currentTurnOrder = 0;
	internal PlayerId NewPlayerId => new PlayerId(_playerIdCounter++);
	internal BattlefieldId NewBattlefieldId => new BattlefieldId(_battlefieldIdCounter++);
	internal UnitId NewUnitId => new UnitId(_unitIdCounter++);

	// Self-contained own usage
	private List<GameIntent> _intentQueue = [];
	private long _playerIdCounter;
	private long _battlefieldIdCounter;
	private long _unitIdCounter;

	// Handle all inputs (oh boy this is gonna be a long function)
	public void Input(PlayerId playerId, PlayerInput input)
	{
		if (input is NextTurnInput nextTurnInput)
		{
			if (playerId == CurrentPlayerIdTurn)
			{
				QueueIntent(new TurnMoveOn());
			}
		}
	}

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
		ActionProcessed?.Invoke(
			(action, true, GameState.FromEngine(this))
		);
	}

	public void StartGame()
	{
		QueueIntent(new SetupGame());
	}

	public Player AddPlayer() // Put card data in here?
	{
		var newId = new PlayerId(_playerIdCounter++);
		var player = new Player(newId);
		player.Name = "Default Name"; // TODO need inject name here
		_players[newId] = player;
		return player;
	}

	internal Battlefield AddBattlefield(BattlefieldId newId)
	{
		var battlefield = new Battlefield(newId);
		_battlefields[newId] = battlefield;
		return battlefield;
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