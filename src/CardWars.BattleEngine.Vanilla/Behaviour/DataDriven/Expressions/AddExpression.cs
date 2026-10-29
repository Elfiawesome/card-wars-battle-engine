using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/add")]
public record struct AddExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class AddExpressionHandler : IExpressionHandler<AddExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, AddExpression request)
	{
		var left = context.Evaluate(request.Left);
		var right = context.Evaluate(request.Right);

		if (left is StringTag || right is StringTag)
			return new StringTag(ExpressionValue.ToText(left) + ExpressionValue.ToText(right));

		if (!ExpressionValue.TryGetNumber(left, out var leftNumber) ||
			!ExpressionValue.TryGetNumber(right, out var rightNumber))
		{
			Log.Error($"Cannot add [{left.Type}] and [{right.Type}]");
			return new NullTag();
		}

		return ExpressionValue.Number(leftNumber + rightNumber, ExpressionValue.PrefersFloat(left, right));
	}
}
