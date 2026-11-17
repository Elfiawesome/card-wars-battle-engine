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
	public DataTag Handle(ScriptContext context, NegateExpression request)
	{
		var value = context.Evaluate(request.Value);
		return value switch
		{
			IntTag intTag => new IntTag(-intTag.Value),
			FloatTag floatTag => new FloatTag(-floatTag.Value),
			_ => NegateFailed(context, value)
		};
	}

	private static DataTag NegateFailed(ScriptContext context, DataTag value)
	{
		context.Logger.Error($"Cannot negate [{value.Type}]");
		return new NullTag();
	}
}
