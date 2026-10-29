using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/variable")]
public record struct VariableExpression(
	[property: DataTag] ResourceId Name
) : IExpression;

public sealed class VariableExpressionHandler : IExpressionHandler<VariableExpression>
{
	public DataTag Handle(ScriptContext context, VariableExpression request)
	{
		if (context.TryGetVariable(request.Name, out var variable)) return variable ?? new NullTag();

		Log.Warn($"Unknown variable [{request.Name}]");
		return new NullTag();
	}
}
