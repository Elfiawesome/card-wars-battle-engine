using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/stage_block")]
public record struct StageBlockInstruction(
	[property: DataTag] IExpression Block
) : IInstruction;

public sealed class StageBlockInstructionHandler : IInstructionHandler<StageBlockInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, StageBlockInstruction request)
	{
		var value = context.Evaluate(request.Block);
		var block = context.ResolveObject<IBlock>(value);
		if (block == null)
		{
			Log.Error($"stage_block expected a block but received [{value.Type}]");
			yield return InstructionOutcome.Continue;
			yield break;
		}

		context.Behaviour.StageBlock(block, context.Behaviour.OwnerEntityId);
		yield return InstructionOutcome.Continue;
	}
}
