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
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, WhileInstruction request)
	{
		var iterations = 0;
		while (context.Evaluate(request.Condition).IsTruthy)
		{
			if (++iterations > context.MaxLoopIterations)
			{
				Log.Error($"While loop exceeded [{context.MaxLoopIterations}] iterations");
				yield break;
			}

			var shouldBreak = false;
			var shouldContinue = false;
			foreach (var outcome in context.ExecuteBlock(request.Body))
			{
				if (outcome == InstructionOutcome.Continue) continue;
				if (outcome == InstructionOutcome.Break)
				{
					shouldBreak = true;
					break;
				}

				if (outcome == InstructionOutcome.ContinueLoop)
				{
					shouldContinue = true;
					break;
				}

				yield return outcome;
			}

			if (shouldBreak) yield break;
			if (shouldContinue) continue;
		}
	}
}
