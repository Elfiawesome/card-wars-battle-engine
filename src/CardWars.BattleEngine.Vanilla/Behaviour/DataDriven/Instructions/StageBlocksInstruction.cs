using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using System.Collections;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/stage_blocks")]
public record struct StageBlocksInstruction(
	[property: DataTag] IExpression Blocks
) : IInstruction;

public sealed class StageBlocksInstructionHandler : IInstructionHandler<StageBlocksInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, StageBlocksInstruction request)
	{
		var value = context.Evaluate(request.Blocks);
		var blocks = ResolveBlocks(context, value);
		if (blocks.Count == 0)
		{
			Log.Error($"stage_blocks expected blocks but received [{value.Type}]");
			yield return InstructionOutcome.Continue;
			yield break;
		}

		context.Behaviour.StageBlocks(new BlockBatch(blocks, context.Behaviour.OwnerEntityId));
		yield return InstructionOutcome.Continue;
	}

	private static List<IBlock> ResolveBlocks(ScriptRuntimeContext context, DataTag value)
	{
		var blocks = new List<IBlock>();
		switch (value)
		{
			case ListTag list:
				foreach (var item in list.Items)
				{
					if (context.ResolveObject<IBlock>(item) is { } block) blocks.Add(block);
				}
				break;
			case ObjectRefTag { Value: IBlock block }:
				blocks.Add(block);
				break;
			case ObjectRefTag { Value: IEnumerable enumerable }:
				foreach (var item in enumerable)
				{
					if (item is IBlock block) blocks.Add(block);
				}
				break;
		}

		return blocks;
	}
}
