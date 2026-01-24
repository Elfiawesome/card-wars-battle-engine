namespace CardWars.BattleEngine.State;

public record struct TurnState(
	List<EntityId> TurnOrder,
	HashSet<EntityId> AllowedPlayerInputs,
	int TurnIndex,
	int TurnNumber, // Total Turn Number
	TurnPhase Phase = TurnPhase.Setup
);

public enum TurnPhase { None = 0, Setup, Attacking }