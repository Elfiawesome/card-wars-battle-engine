using CardWars.BattleEngine.Behaviour.DataDriven.Functions;
using CardWars.BattleEngine.Behaviour.DataDriven.Objects;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven;

public sealed class DataDrivenRegistry
{
	public HandlerRegistry<ScriptContext, DataTag> ExpressionHandlers { get; } = new();
	public HandlerRegistry<ScriptRuntimeContext, IEnumerable<InstructionOutcome>> InstructionHandlers { get; } = new();
	public ObjectAccessRegistry ObjectAccessors { get; } = new();
	public Registry<ResourceId, IDataDrivenFunction> Functions { get; } = new();

	public DataDrivenRegistry()
	{
		ObjectAccessors.Register<GameState>(new GameStateObjectAccessor());
		ObjectAccessors.Register<CompoundTag>(new TagObjectAccessor());
		ObjectAccessors.Register<ListTag>(new TagObjectAccessor());
		ObjectAccessors.Register<ScriptContext>(new RuntimeVariablesObjectAccessor());
	}
}
