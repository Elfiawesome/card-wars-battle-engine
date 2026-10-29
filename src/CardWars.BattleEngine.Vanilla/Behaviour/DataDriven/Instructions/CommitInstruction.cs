using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/commit")]
public record struct CommitInstruction() : IInstruction;

public sealed class CommitInstructionHandler : IInstructionHandler<CommitInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, CommitInstruction request)
	{
		context.Behaviour.CommitStagedBlocks();
		yield return InstructionOutcome.Continue;
	}
}
