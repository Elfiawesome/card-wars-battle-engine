using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/list")]
public record struct ListExpression(
	[property: DataTag] List<IExpression>? Items = null
) : IExpression;

public sealed class ListExpressionHandler : IExpressionHandler<ListExpression>
{
	public DataTag Handle(ScriptContext context, ListExpression request)
	{
		var list = new ListTag();
		foreach (var item in request.Items ?? [])
		{
			list.Add(context.Evaluate(item));
		}

		return list;
	}
}
