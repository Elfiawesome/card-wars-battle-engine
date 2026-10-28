using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/raise")]
public record struct RaiseInstruction(
	[property: DataTag] IExpression Event
) : IInstruction;

public sealed class RaiseInstructionHandler : IInstructionHandler<RaiseInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, RaiseInstruction request)
	{
		var value = context.Evaluate(request.Event);
		var evnt = context.ResolveObject<IEvent>(value);
		if (evnt == null)
		{
			Log.Error($"raise expected an event but received [{value.Type}]");
			return InstructionOutcome.Continue;
		}

		context.Behaviour.RaiseEvent(evnt);
		return InstructionOutcome.Continue;
	}
}
