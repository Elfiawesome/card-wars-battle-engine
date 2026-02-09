namespace CardWars.BattleEngine.State;

public record struct TurnState(
	List<EntityId> TurnOrder,
	HashSet<EntityId> AllowedPlayerInputs,
	int TurnIndex,
	int TurnNumber, // Total Turn Number
	TurnPhase Phase = TurnPhase.Setup
)
{
	public TurnState Copy() => new()
	{
		TurnOrder = [.. this.TurnOrder],
		AllowedPlayerInputs = [.. this.AllowedPlayerInputs],
		TurnIndex = this.TurnIndex,
		TurnNumber = this.TurnNumber,
		Phase = this.Phase
	};
};

public enum TurnPhase { None = 0, Setup, Attacking }