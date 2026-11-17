using CardWars.BattleEngine.State;
using CardWars.Core.Data;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Input;

public record struct InputContext(
	Transaction Transaction,
	EntityId PlayerId
)
{
	/// <summary>The owning transaction's data context (ultimately the engine's own context).</summary>
	public readonly DataContext Data => Transaction.Data;

	/// <summary>The owning transaction's logger (ultimately the engine's own logger).</summary>
	public readonly ILogger Logger => Transaction.Logger;
}