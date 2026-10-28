using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/while")]
public record struct WhileInstruction(
	[property: DataTag] IExpression Condition,
	[property: DataTag] List<IInstruction>? Body = null
) : IInstruction;

public sealed class WhileInstructionHandler : IInstructionHandler<WhileInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, WhileInstruction request)
	{
		var iterations = 0;
		while (context.Evaluate(request.Condition).IsTruthy)
		{
			if (++iterations > context.MaxLoopIterations)
			{
				Log.Error($"While loop exceeded [{context.MaxLoopIterations}] iterations");
				return InstructionOutcome.Continue;
			}

			var outcome = context.ExecuteBlock(request.Body);
			if (outcome == InstructionOutcome.Break) break;
			if (outcome == InstructionOutcome.ContinueLoop) continue;
			if (outcome != InstructionOutcome.Continue) return outcome;
		}

		return InstructionOutcome.Continue;
	}
}
