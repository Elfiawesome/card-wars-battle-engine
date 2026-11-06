using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.State;

public sealed class TurnState
{
	[DataTag] public List<EntityId> TurnOrder { get; set; } = [];
	[DataTag] public HashSet<EntityId> AllowedPlayerInputs { get; set; } = [];
	[DataTag] public int TurnIndex { get; set; }
	[DataTag] public int TurnNumber { get; set; }
	[DataTag] public int RoundNumber { get; set; } = 1;
	[DataTag] public TurnPhase Phase { get; set; } = TurnPhase.Setup;

	public TurnState()
	{
	}

	public TurnState(
		List<EntityId>? turnOrder = null,
		HashSet<EntityId>? allowedPlayerInputs = null,
		int turnIndex = 0,
		int turnNumber = 0,
		TurnPhase phase = TurnPhase.Setup,
		int roundNumber = 1)
	{
		TurnOrder = turnOrder ?? [];
		AllowedPlayerInputs = allowedPlayerInputs ?? [];
		TurnIndex = turnIndex;
		TurnNumber = turnNumber;
		Phase = phase;
		RoundNumber = roundNumber;
	}

	public TurnState Clone() => new(
		[.. TurnOrder],
		[.. AllowedPlayerInputs],
		TurnIndex,
		TurnNumber,
		Phase,
		RoundNumber
	);

	public bool Equals(TurnState? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;

		return TurnIndex == other.TurnIndex
			&& TurnNumber == other.TurnNumber
			&& RoundNumber == other.RoundNumber
			&& Phase == other.Phase
			&& TurnOrder.SequenceEqual(other.TurnOrder)
			&& AllowedPlayerInputs.SetEquals(other.AllowedPlayerInputs);
	}

}

public enum TurnPhase { None = 0, Setup, Attacking }
