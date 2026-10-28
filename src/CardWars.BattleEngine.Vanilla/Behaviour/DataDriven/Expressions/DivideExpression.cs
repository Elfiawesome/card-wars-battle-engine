using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/divide")]
public record struct DivideExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class DivideExpressionHandler : IExpressionHandler<DivideExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, DivideExpression request)
	{
		Log.Error("DivideExpression is not implemented");
		return new NullTag();
	}
}
