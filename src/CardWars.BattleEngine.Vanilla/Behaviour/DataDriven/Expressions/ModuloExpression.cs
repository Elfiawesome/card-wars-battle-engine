using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/modulo")]
public record struct ModuloExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class ModuloExpressionHandler : IExpressionHandler<ModuloExpression>
{
	public DataTag Handle(ScriptContext context, ModuloExpression request)
	{
		var left = context.Evaluate(request.Left);
		var right = context.Evaluate(request.Right);

		if (!ExpressionValue.TryGetNumber(left, out var leftNumber) ||
			!ExpressionValue.TryGetNumber(right, out var rightNumber))
		{
			Log.Error($"Cannot modulo [{left.Type}] by [{right.Type}]");
			return new NullTag();
		}

		if (rightNumber == 0)
		{
			Log.Error("Cannot modulo by zero");
			return new NullTag();
		}

		return ExpressionValue.Number(leftNumber % rightNumber, ExpressionValue.PrefersFloat(left, right));
	}
}
