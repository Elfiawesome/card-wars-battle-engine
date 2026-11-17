using System.Collections;
using CardWars.BattleEngine.Behaviour.DataDriven.Instructions;
using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Instructions;

[DataTagType("script/instruction/log")]
public record struct LogInstruction(
	[property: DataTag] IExpression Value
) : IInstruction;

public sealed class LogInstructionHandler : IInstructionHandler<LogInstruction>
{
	public IEnumerable<InstructionOutcome> Handle(ScriptRuntimeContext context, LogInstruction request)
	{
		var value = context.Evaluate(request.Value);
		// R9: the logging layer no longer serializes values. Format the evaluated value to a
		// readable string HERE, using this script's own DataContext, then log a plain string.
		var message = ToReadable(context.Data, value is ObjectRefTag objectRef ? objectRef.Value : value);
		context.Logger.Info(message);
		yield return InstructionOutcome.Continue;
	}

	// Mirrors the formatting the old static Log.ToReadable performed, but routes serialization
	// through the caller-supplied DataContext instead of the data-layer-coupled logger (R9).
	private static string ToReadable(DataContext data, object? obj)
	{
		if (obj is null) return "null";

		if (obj is string s) return s;
		if (obj is int || obj is float || obj is bool || obj is Guid)
			return obj.ToString() ?? "";

		if (obj is IEnumerable enumerable)
			return "[" + string.Join(", ", enumerable.Cast<object>().Select(item => ToReadable(data, item))) + "]";

		if (obj is ObjectRefTag objectRef) return objectRef.Value.ToString() ?? "<object>";

		if (obj is DataTag tag)
			return data.Serialize(tag);  // compact JSON

		var compound = data.ToTag(obj, includeType: true);
		if (compound.Count == 0) return obj.ToString() ?? "<NULL INSTANCE>";
		return data.Serialize(compound);
	}
}
