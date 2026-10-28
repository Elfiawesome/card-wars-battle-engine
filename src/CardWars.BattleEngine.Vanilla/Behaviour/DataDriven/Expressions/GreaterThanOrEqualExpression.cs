using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/greater_than_or_equal")]
public record struct GreaterThanOrEqualExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class GreaterThanOrEqualExpressionHandler : IExpressionHandler<GreaterThanOrEqualExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, GreaterThanOrEqualExpression request)
	{
		Log.Error("GreaterThanOrEqualExpression is not implemented");
		return new NullTag();
	}
}
