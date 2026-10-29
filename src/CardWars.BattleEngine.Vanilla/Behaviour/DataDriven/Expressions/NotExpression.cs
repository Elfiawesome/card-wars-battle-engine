using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/not")]
public record struct NotExpression(
	[property: DataTag] IExpression Value
) : IExpression;

public sealed class NotExpressionHandler : IExpressionHandler<NotExpression>
{
	public DataTag Handle(ScriptContext context, NotExpression request)
	{
		var value = context.Evaluate(request.Value);
		return new BoolTag(!value.IsTruthy);
	}
}
