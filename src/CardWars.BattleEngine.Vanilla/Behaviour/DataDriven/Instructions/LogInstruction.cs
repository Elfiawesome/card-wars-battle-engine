using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/log")]
public record struct LogInstruction(
	[property: DataTag] IExpression Value
) : IInstruction;

public sealed class LogInstructionHandler : IInstructionHandler<LogInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, LogInstruction request)
	{
		var value = context.Evaluate(request.Value);
		Log.Info(value is ObjectRefTag objectRef ? objectRef.Value : value);
		yield return InstructionOutcome.Continue;
	}
}
