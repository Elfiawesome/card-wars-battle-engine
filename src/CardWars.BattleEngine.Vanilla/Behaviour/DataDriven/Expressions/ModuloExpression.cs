using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/modulo")]
public record struct ModuloExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class ModuloExpressionHandler : IExpressionHandler<ModuloExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, ModuloExpression request)
	{
		Log.Error("ModuloExpression is not implemented");
		return new NullTag();
	}
}
