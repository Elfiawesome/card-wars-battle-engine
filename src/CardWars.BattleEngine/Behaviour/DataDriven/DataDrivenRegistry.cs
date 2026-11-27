using CardWars.BattleEngine.Behaviour.DataDriven.Functions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public sealed class DataDrivenRegistry(DataTagContext dataTags, Logger logger)
{
	public DataTagContext DataTags { get; } = dataTags;
	public Logger Logger { get; } = logger;
	public HandlerRegistry<ScriptContext, DataTag> ExpressionHandlers { get; } = new();
	public HandlerRegistry<ScriptRuntimeContext, IEnumerable<InstructionOutcome>> InstructionHandlers { get; } = new();
	public Registry<ResourceId, IDataDrivenFunction> Functions { get; } = new();
}
