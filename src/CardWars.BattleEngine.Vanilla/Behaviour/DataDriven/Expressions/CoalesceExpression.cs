using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/coalesce")]
public record struct CoalesceExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class CoalesceExpressionHandler : IExpressionHandler<CoalesceExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, CoalesceExpression request)
	{
		Log.Error("CoalesceExpression is not implemented");
		return new NullTag();
	}
}
