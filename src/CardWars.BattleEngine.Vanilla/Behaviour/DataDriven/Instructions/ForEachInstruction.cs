using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using System.Collections;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Mapping;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/for_each")]
public record struct ForEachInstruction(
	[property: DataTag] ResourceId Variable,
	[property: DataTag] IExpression Collection,
	[property: DataTag] List<IInstruction>? Body = null
) : IInstruction;

public sealed class ForEachInstructionHandler : IInstructionHandler<ForEachInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, ForEachInstruction request)
	{
		var collection = context.Evaluate(request.Collection);
		var items = GetItems(collection);
		if (items == null)
		{
			Log.Error($"ForEach collection type [{collection.Type}] is not enumerable");
			return InstructionOutcome.Continue;
		}

		context.PushScope();
		try
		{
			var iterations = 0;
			foreach (var item in items)
			{
				if (++iterations > context.MaxLoopIterations)
				{
					Log.Error($"ForEach loop exceeded [{context.MaxLoopIterations}] iterations");
					return InstructionOutcome.Continue;
				}

				context.DeclareVariable(request.Variable, item);
				var outcome = context.ExecuteBlock(request.Body);
				if (outcome == InstructionOutcome.Break) break;
				if (outcome == InstructionOutcome.ContinueLoop) continue;
				if (outcome != InstructionOutcome.Continue) return outcome;
			}

			return InstructionOutcome.Continue;
		}
		finally
		{
			context.PopScope();
		}
	}

	private static IEnumerable<DataTag>? GetItems(DataTag collection) => collection switch
	{
		ListTag list => list.Items,
		ObjectRefTag { Value: IEnumerable enumerable } => enumerable
			.Cast<object?>()
			.Select(item => item is DataTag tag ? tag : DataTagMapper.ToValueTag(item) ?? new NullTag()),
		_ => null
	};
}
