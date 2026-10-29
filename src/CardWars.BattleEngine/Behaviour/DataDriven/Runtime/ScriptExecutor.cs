using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Runtime;

public sealed class ScriptExecutor(
	ScriptRuntimeContext context,
	HandlerRegistry<ScriptRuntimeContext, IEnumerable<InstructionOutcome>> handlers)
{
	public IEnumerable<InstructionOutcome> Execute(IReadOnlyList<IInstruction>? instructions)
	{
		if (instructions is null) yield break;

		foreach (var instruction in instructions)
		{
			if (!handlers.TryExecute(context, instruction, out var outcomes) || outcomes is null)
				yield break;

			foreach (var outcome in outcomes)
			{
				yield return outcome;

				if (outcome == InstructionOutcome.WaitForInput) continue;
				if (outcome != InstructionOutcome.Continue) yield break;
			}
		}
	}
}
