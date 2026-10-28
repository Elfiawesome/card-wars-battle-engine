using System.Reflection;
using CardWars.BattleEngine.Behaviour.DataDriven;
using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.BattleEngine.Event;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;



[DataTagType("script/instruction/cancel")]
public record struct CancelInstruction() : IInstruction;

public sealed class CancelInstructionHandler : IInstructionHandler<CancelInstruction>
{
	public InstructionOutcome Handle(ScriptRuntimeContext context, CancelInstruction request)
	{
		var evnt = context.TryGetVariable(ScriptBindings.Event, out var eventTag)
			? context.ResolveObject<IEvent>(eventTag)
			: null;

		var property = evnt?.GetType().GetProperty("IsCancelled", BindingFlags.Public | BindingFlags.Instance);
		if (property is { CanWrite: true } && property.PropertyType == typeof(bool))
		{
			property.SetValue(evnt, true);
			return InstructionOutcome.Continue;
		}

		Log.Warn($"Event [{evnt?.GetType().Name ?? "none"}] cannot be cancelled");
		return InstructionOutcome.Continue;
	}
}
