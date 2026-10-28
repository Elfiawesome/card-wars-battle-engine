using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/set")]
public record struct SetInstruction(
	[property: DataTag] ResourceId Name,
	[property: DataTag] IExpression Value
) : IInstruction;

public sealed class SetInstructionHandler : IInstructionHandler<SetInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, SetInstruction request)
	{
		context.TrySetVariable(request.Name, context.Evaluate(request.Value));
		return InstructionOutcome.Continue;
	}
}
