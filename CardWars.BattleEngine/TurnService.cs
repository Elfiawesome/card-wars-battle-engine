using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine;

public class TurnService
{
	public enum PhaseType { Setup, Attacking }

	public HashSet<PlayerId> AllowedPlayerInput = [];
	public IReadOnlyList<PlayerId> TurnOrder => _turnOrder;
	public PlayerId? CurrentPlayerId
	{
		get
		{
			if (TurnNumber < _turnOrder.Count || TurnNumber >= 0) { return _turnOrder[TurnNumber]; }
			return null;
		}
	}
	public int TurnNumber { get; private set; }
	public PhaseType Phase { get; private set; } = PhaseType.Setup;

	private readonly List<PlayerId> _turnOrder = [];

	public TurnService()
	{
	}

	public void AddPlayer(PlayerId playerId)
	{
		_turnOrder.Add(playerId);
	}

	public void RemovePlayer(PlayerId playerId)
	{
		_turnOrder.RemoveAll((s) => s == playerId);
	}

	public void NextTurn()
	{
		TurnNumber++;
		if (TurnNumber >= _turnOrder.Count)
		{
			TurnNumber = 0;
			NextPhase();
		}
		AllowedPlayerInput.Clear();
		if (CurrentPlayerId != null) { AllowedPlayerInput.Add((PlayerId)CurrentPlayerId); }
	}

	public void NextPhase()
	{
		Phase = (Phase == PhaseType.Setup) ? PhaseType.Attacking : PhaseType.Setup;
	}

	public bool IsPlayerInputAllowed(PlayerId playerId) => AllowedPlayerInput.Contains(playerId);
}