using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/negate")]
public record struct NegateExpression(
	[property: DataTag] IExpression Value
) : IExpression;

public sealed class NegateExpressionHandler : IExpressionHandler<NegateExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, NegateExpression request)
	{
		Log.Error("NegateExpression is not implemented");
		return new NullTag();
	}
}
