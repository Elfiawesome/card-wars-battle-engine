using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/conditional")]
public record struct ConditionalExpression(
	[property: DataTag] IExpression Condition,
	[property: DataTag] IExpression Then,
	[property: DataTag] IExpression Else
) : IExpression;

public sealed class ConditionalExpressionHandler : IExpressionHandler<ConditionalExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, ConditionalExpression request)
	{
		var condition = context.Evaluate(request.Condition);
		return condition.IsTruthy
			? context.Evaluate(request.Then)
			: context.Evaluate(request.Else);
	}
}
