using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/continue")]
public record struct ContinueInstruction() : IInstruction;

public sealed class ContinueInstructionHandler : IInstructionHandler<ContinueInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, ContinueInstruction request)
		=> InstructionOutcome.ContinueLoop;
}
