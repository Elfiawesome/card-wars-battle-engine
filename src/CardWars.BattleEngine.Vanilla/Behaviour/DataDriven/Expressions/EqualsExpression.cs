using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/equals")]
public record struct EqualsExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class EqualsExpressionHandler : IExpressionHandler<EqualsExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, EqualsExpression request)
	{
		Log.Error("EqualsExpression is not implemented");
		return new NullTag();
	}
}
