using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/literal")]
public record struct LiteralExpression(
	[property: DataTag] DataTag Value
) : IExpression;

public sealed class LiteralExpressionHandler : IExpressionHandler<LiteralExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, LiteralExpression request)
		=> request.Value.Clone();
}
