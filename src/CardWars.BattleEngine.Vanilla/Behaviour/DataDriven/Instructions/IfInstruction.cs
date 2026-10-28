using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/if")]
public record struct IfInstruction(
	[property: DataTag] IExpression Condition,
	[property: DataTag] List<IInstruction>? Then = null,
	[property: DataTag] List<IInstruction>? Else = null
) : IInstruction;

public sealed class IfInstructionHandler : IInstructionHandler<IfInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, IfInstruction request)
	{
		var condition = context.Evaluate(request.Condition);
		var instructions = condition.IsTruthy ? request.Then : request.Else;
		return context.ExecuteBlock(instructions);
	}
}
