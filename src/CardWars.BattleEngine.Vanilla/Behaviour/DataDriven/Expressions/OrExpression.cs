using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/or")]
public record struct OrExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class OrExpressionHandler : IExpressionHandler<OrExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, OrExpression request)
	{
		Log.Error("OrExpression is not implemented");
		return new NullTag();
	}
}
