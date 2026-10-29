using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/break")]
public record struct BreakInstruction() : IInstruction;

public sealed class BreakInstructionHandler : IInstructionHandler<BreakInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, BreakInstruction request)
	{
		yield return InstructionOutcome.Break;
	}
}
