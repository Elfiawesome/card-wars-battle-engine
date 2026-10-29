using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/and")]
public record struct AndExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class AndExpressionHandler : IExpressionHandler<AndExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, AndExpression request)
	{
		if (!context.Evaluate(request.Left).IsTruthy)
			return new BoolTag(false);

		return new BoolTag(context.Evaluate(request.Right).IsTruthy);
	}
}
