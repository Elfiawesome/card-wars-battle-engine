using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/not_equals")]
public record struct NotEqualsExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class NotEqualsExpressionHandler : IExpressionHandler<NotEqualsExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, NotEqualsExpression request)
	{
		Log.Error("NotEqualsExpression is not implemented");
		return new NullTag();
	}
}
