using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/object")]
public record struct ObjectExpression(
	[property: DataTag] Dictionary<string, IExpression>? Entries = null
) : IExpression;

public sealed class ObjectExpressionHandler : IExpressionHandler<ObjectExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, ObjectExpression request)
	{
		var compound = new CompoundTag();
		foreach (var (key, item) in request.Entries ?? [])
		{
			compound.Set(key, context.Evaluate(item));
		}

		return compound;
	}
}
