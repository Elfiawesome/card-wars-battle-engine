using CardWars.Core.Data;

namespace CardWars.BattleEngine.State;

public record struct TurnState(
    [property: DataTag] List<EntityId> TurnOrder,
    [property: DataTag] HashSet<EntityId> AllowedPlayerInputs,
    [property: DataTag] int TurnIndex,
    [property: DataTag] int TurnNumber,
    [property: DataTag] TurnPhase Phase = TurnPhase.Setup
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