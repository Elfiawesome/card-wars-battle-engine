using CardWars.BattleEngine.Behaviour;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.Rules;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine;

// The owner (an engine/server/session with a real ILogger) may supply its logger so the handler
// registries' dispatch Debug/Error diagnostics reach the owner's console sink; otherwise the
// immutable no-op NullLogger.Instance is used. No mutable static state is read (R1).
public class BattleEngineRegistry(ILogger? logger = null)
{
	// The owner's logger (or the no-op NullLogger.Instance when none was supplied). Exposed so
	// mod-load code (e.g. the vanilla mod's Register* helpers) can route its "Registered ..."
	// diagnostics through the owning session's sink instead of always falling back to
	// NullLogger.Instance (R9). Immutable after construction; no mutable static state (R1).
	public ILogger Logger { get; } = logger ?? NullLogger.Instance;

	public HandlerRegistry<InputContext> InputHandlers = new(logger);
	public HandlerRegistry<GameState> BlockHandlers = new(logger);
	public HandlerRegistry<Transaction> EventHandlers = new(logger);
	public DataDrivenRegistry DataDriven { get; } = new(logger);
	public RegistryFactory<ResourceId, IBehaviour> Behaviours { get; } = new();
	public RegistryFactory<ResourceId, EntityId, IEntity> Entities { get; } = new();
	public RegistryFactory<ResourceId, IEntityCondition> EntityConditions { get; } = new();

	public Registry<ResourceId, CompoundTag> CardDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> AttackDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> RuleDefinitions { get; } = new();
	public Registry<ResourceId, CompoundTag> AbilityTypeDefinitions { get; } = new(); // TODO: Should this be here???
	public Registry<ResourceId, object> CustomContent {get;} = new(); // Other modded data content
}
