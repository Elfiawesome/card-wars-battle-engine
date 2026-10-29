using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Runtime;

public sealed class ScriptRuntimeContext : ScriptContext
{
	public BehaviourContext Behaviour { get; }
	public ScriptExecutor Executor { get; }

	public ScriptRuntimeContext(BehaviourContext behaviour, DataDrivenRegistry dataDriven)
		: base(behaviour.State, dataDriven)
	{
		Behaviour = behaviour;
		Executor = new ScriptExecutor(this, dataDriven.InstructionHandlers);
	}

	public IEnumerable<InstructionOutcome> ExecuteBlock(IReadOnlyList<IInstruction>? instructions)
	{
		if (instructions is null) yield break;

		PushScope();
		try
		{
			foreach (var outcome in Executor.Execute(instructions))
				yield return outcome;
		}
		finally
		{
			PopScope();
		}
	}
}
