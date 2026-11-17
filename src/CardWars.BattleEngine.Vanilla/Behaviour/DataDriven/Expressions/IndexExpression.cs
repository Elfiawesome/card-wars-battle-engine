using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/index")]
public record struct IndexExpression(
	[property: DataTag] IExpression Target,
	[property: DataTag] IExpression Index
) : IExpression;

public sealed class IndexExpressionHandler : IExpressionHandler<IndexExpression>
{
	public DataTag Handle(ScriptContext context, IndexExpression request)
	{
		var target = context.Evaluate(request.Target);
		var index = context.Evaluate(request.Index);
		var targetObject = target is ObjectRefTag objectRef ? objectRef.Value : target;

		if (targetObject is NullTag)
		{
			context.Logger.Warn("Cannot index a null value");
			return new NullTag();
		}

		if (context.Data.TryGetIndex(targetObject, index, out var value))
			return value ?? new NullTag();

		context.Logger.Warn($"Index [{index.Type}] was not found on [{targetObject.GetType().Name}]");
		return new NullTag();
	}
}
