using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/declare")]
public record struct DeclarationInstruction(
	[property: DataTag] ResourceId Name,
	[property: DataTag] IExpression Value,
	[property: DataTag] bool IsConstant = false
) : IInstruction;

public sealed class DeclarationInstructionHandler : IInstructionHandler<DeclarationInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, DeclarationInstruction request)
	{
		context.DeclareVariable(request.Name, context.Evaluate(request.Value), request.IsConstant);
		return InstructionOutcome.Continue;
	}
}
