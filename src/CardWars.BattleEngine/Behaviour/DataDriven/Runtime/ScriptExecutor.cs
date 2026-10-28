using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Runtime;

public sealed class ScriptExecutor(
	ScriptRuntimeContext context,
	HandlerRegistry<ScriptRuntimeContext, InstructionOutcome> handlers)
{
	public InstructionOutcome Execute(IReadOnlyList<IInstruction>? instructions)
	{
		if (instructions == null) return InstructionOutcome.Continue;

		foreach (var instruction in instructions)
		{
			var outcome = handlers.Execute(context, instruction);
			if (outcome != InstructionOutcome.Continue) return outcome;
		}

		return InstructionOutcome.Continue;
	}
}
