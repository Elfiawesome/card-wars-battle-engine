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
	public InstructionOutcome Handle(ScriptRuntimeContext context, ForInstruction request)
	{
		context.PushScope();
		try
		{
			if (request.Init != null)
			{
				var init = context.Executor.Execute([request.Init]);
				if (init != InstructionOutcome.Continue) return init;
			}

			var iterations = 0;
			while (request.Condition == null || context.Evaluate(request.Condition).IsTruthy)
			{
				if (++iterations > context.MaxLoopIterations)
				{
					Log.Error($"For loop exceeded [{context.MaxLoopIterations}] iterations");
					return InstructionOutcome.Continue;
				}

				var outcome = context.ExecuteBlock(request.Body);
				if (outcome == InstructionOutcome.Break) break;
				if (outcome != InstructionOutcome.Continue && outcome != InstructionOutcome.ContinueLoop) return outcome;

				if (request.Step != null)
				{
					var step = context.Executor.Execute([request.Step]);
					if (step != InstructionOutcome.Continue && step != InstructionOutcome.ContinueLoop) return step;
				}
			}

			return InstructionOutcome.Continue;
		}
		finally
		{
			context.PopScope();
		}
	}
}
