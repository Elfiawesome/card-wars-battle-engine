using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Request;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Instructions;

public interface IInstructionHandler<TInstruction> : IRequestHandler<ScriptRuntimeContext, TInstruction, InstructionOutcome>
	where TInstruction : IInstruction;
