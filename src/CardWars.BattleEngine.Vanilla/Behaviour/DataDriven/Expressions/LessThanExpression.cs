using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/less_than")]
public record struct LessThanExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class LessThanExpressionHandler : IExpressionHandler<LessThanExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, LessThanExpression request)
	{
		var left = context.Evaluate(request.Left);
		var right = context.Evaluate(request.Right);
		var comparison = ExpressionValue.Compare(left, right);
		if (comparison == null)
		{
			Log.Error($"Cannot compare [{left.Type}] and [{right.Type}]");
			return new NullTag();
		}

		return new BoolTag(comparison < 0);
	}
}
