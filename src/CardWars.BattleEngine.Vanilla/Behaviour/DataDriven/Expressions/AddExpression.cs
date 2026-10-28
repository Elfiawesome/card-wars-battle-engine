using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/add")]
public record struct AddExpression(
	[property: DataTag] IExpression Left,
	[property: DataTag] IExpression Right
) : IExpression;

public sealed class AddExpressionHandler : IExpressionHandler<AddExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, AddExpression request)
	{
		Log.Error("AddExpression is not implemented");
		return new NullTag();
	}
}
