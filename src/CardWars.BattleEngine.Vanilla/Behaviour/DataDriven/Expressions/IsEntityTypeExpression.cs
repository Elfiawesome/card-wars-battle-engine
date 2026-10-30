using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/is_entity_type")]
public record struct IsEntityTypeExpression(
	[property: DataTag] IExpression Value,
	[property: DataTag] ResourceId EntityType
) : IExpression;

public sealed class IsEntityTypeExpressionHandler : IExpressionHandler<IsEntityTypeExpression>
{
	public DataTag Handle(ScriptContext context, IsEntityTypeExpression request)
	{
		var value = context.Evaluate(request.Value);
		if (value is not ObjectRefTag objectRef) return new BoolTag(false);

		var typeId = DataTagTypeRegistry.GetTypeId(objectRef.Value.GetType());
		return new BoolTag(typeId == request.EntityType.ToString());
	}
}
