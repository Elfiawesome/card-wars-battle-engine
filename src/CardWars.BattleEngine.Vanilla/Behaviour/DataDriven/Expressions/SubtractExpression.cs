using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/subtract")]
public record struct SubtractExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class SubtractExpressionHandler : IExpressionHandler<SubtractExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, SubtractExpression request)
	{
		Log.Error("SubtractExpression is not implemented");
		return new NullTag();
	}
}
