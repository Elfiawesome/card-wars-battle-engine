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
		Log.Error("LessThanExpression is not implemented");
		return new NullTag();
	}
}
