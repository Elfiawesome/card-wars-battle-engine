using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Functions;

public interface IDataDrivenFunction
{
	DataTag Invoke(ScriptRuntimeContext context, IReadOnlyList<DataTag> arguments);
}
