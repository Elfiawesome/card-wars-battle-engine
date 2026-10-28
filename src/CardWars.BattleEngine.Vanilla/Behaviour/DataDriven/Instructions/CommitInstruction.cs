using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/commit")]
public record struct CommitInstruction() : IInstruction;

public sealed class CommitInstructionHandler : IInstructionHandler<CommitInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, CommitInstruction request)
	{
		context.Behaviour.CommitStagedBlocks();
		return InstructionOutcome.Continue;
	}
}
