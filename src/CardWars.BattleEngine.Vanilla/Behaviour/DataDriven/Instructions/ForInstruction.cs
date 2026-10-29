using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/for")]
public record struct ForInstruction(
	[property: DataTag] IInstruction? Init = null,
	[property: DataTag] IExpression? Condition = null,
	[property: DataTag] IInstruction? Step = null,
	[property: DataTag] List<IInstruction>? Body = null
) : IInstruction;

public sealed class ForInstructionHandler : IInstructionHandler<ForInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, ForInstruction request)
	{
		context.PushScope();
		try
		{
			if (request.Init != null)
			{
				foreach (var outcome in ExecuteSingle(context, request.Init))
				{
					yield return outcome;
					if (outcome != InstructionOutcome.Continue && outcome != InstructionOutcome.WaitForInput) yield break;
				}
			}

			var iterations = 0;
			while (request.Condition == null || context.Evaluate(request.Condition).IsTruthy)
			{
				if (++iterations > context.MaxLoopIterations)
				{
					Log.Error($"For loop exceeded [{context.MaxLoopIterations}] iterations");
					yield break;
				}

				var shouldBreak = false;
				foreach (var outcome in context.ExecuteBlock(request.Body))
				{
					if (outcome == InstructionOutcome.Continue) continue;
					if (outcome == InstructionOutcome.Break)
					{
						shouldBreak = true;
						break;
					}

					if (outcome == InstructionOutcome.ContinueLoop) break;

					yield return outcome;
				}

				if (shouldBreak) yield break;

				if (request.Step != null)
				{
					foreach (var outcome in ExecuteSingle(context, request.Step))
					{
						yield return outcome;
						if (outcome != InstructionOutcome.Continue && outcome != InstructionOutcome.WaitForInput) yield break;
					}
				}
			}
		}
		finally
		{
			context.PopScope();
		}
	}

	private static IEnumerable<InstructionOutcome> ExecuteSingle(ScriptRuntimeContext context, IInstruction instruction)
		=> context.Executor.Execute([instruction]);
}
