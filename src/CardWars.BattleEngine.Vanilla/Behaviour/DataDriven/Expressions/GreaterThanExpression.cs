using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/greater_than")]
public record struct GreaterThanExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class GreaterThanExpressionHandler : IExpressionHandler<GreaterThanExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, GreaterThanExpression request)
	{
		Log.Error("GreaterThanExpression is not implemented");
		return new NullTag();
	}
}
