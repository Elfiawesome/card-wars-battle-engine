using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/equals")]
public record struct EqualsExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class EqualsExpressionHandler : IExpressionHandler<EqualsExpression>
{
	public DataTag Handle(ScriptContext context, EqualsExpression request)
	{
		var left = context.Evaluate(request.Left);
		var right = context.Evaluate(request.Right);
		return new BoolTag(ExpressionValue.AreEqual(left, right));
	}
}
