using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine;

public sealed class TurnService
{
	public enum PhaseType { Setup, Attacking }

	public HashSet<PlayerId> AllowedPlayerInput = [];
	public IReadOnlyList<PlayerId> TurnOrder => _turnOrder;
	public PlayerId? CurrentPlayerId => GetPlayerByTurnNumber(TurnNumber);
	public int TurnNumber { get; set; }
	public PhaseType Phase { get; set; } = PhaseType.Setup;

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

	public bool IsPlayerInputAllowed(PlayerId playerId) => AllowedPlayerInput.Contains(playerId);

	public PlayerId? GetPlayerByTurnNumber(int turnNumber)
	{
		if (turnNumber < _turnOrder.Count && turnNumber >= 0 && _turnOrder.Count > 0) { return _turnOrder[turnNumber]; }
		return null;
	}

}