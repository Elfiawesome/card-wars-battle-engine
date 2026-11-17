using CardWars.BattleEngine.Behaviour.DataDriven.Functions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

// The owner's ILogger (where one exists) is threaded into the dispatch registries so their
// Debug/Error diagnostics reach the owner's sink; otherwise NullLogger.Instance (R1).
public sealed class DataDrivenRegistry(ILogger? logger = null)
{
	public HandlerRegistry<ScriptContext, DataTag> ExpressionHandlers { get; } = new(logger);
	public HandlerRegistry<ScriptRuntimeContext, IEnumerable<InstructionOutcome>> InstructionHandlers { get; } = new(logger);
	public Registry<ResourceId, IDataDrivenFunction> Functions { get; } = new();
}
