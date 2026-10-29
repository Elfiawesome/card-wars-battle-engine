using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/wait")]
public record struct WaitInstruction() : IInstruction;

public sealed class WaitInstructionHandler : IInstructionHandler<WaitInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, WaitInstruction request)
	{
		context.Behaviour.CommitStagedBlocks();
		yield return InstructionOutcome.WaitForInput;
	}
}
