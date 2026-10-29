using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/subtract")]
public record struct SubtractExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class SubtractExpressionHandler : IExpressionHandler<SubtractExpression>
{
	public DataTag Handle(ScriptContext context, SubtractExpression request)
	{
		var left = context.Evaluate(request.Left);
		var right = context.Evaluate(request.Right);

		if (!ExpressionValue.TryGetNumber(left, out var leftNumber) ||
			!ExpressionValue.TryGetNumber(right, out var rightNumber))
		{
			Log.Error($"Cannot subtract [{right.Type}] from [{left.Type}]");
			return new NullTag();
		}

		return ExpressionValue.Number(leftNumber - rightNumber, ExpressionValue.PrefersFloat(left, right));
	}
}
