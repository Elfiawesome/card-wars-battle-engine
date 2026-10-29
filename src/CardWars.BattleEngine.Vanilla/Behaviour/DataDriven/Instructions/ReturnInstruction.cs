using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/return")]
public record struct ReturnInstruction(
	[property: DataTag] IExpression? Value = null
) : IInstruction;

public sealed class ReturnInstructionHandler : IInstructionHandler<ReturnInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, ReturnInstruction request)
	{
		if (request.Value is not null) context.Evaluate(request.Value);
		yield return InstructionOutcome.Return;
	}
}
