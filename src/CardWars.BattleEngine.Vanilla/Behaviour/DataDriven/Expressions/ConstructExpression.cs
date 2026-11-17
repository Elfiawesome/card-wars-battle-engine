using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/construct")]
public record struct ConstructExpression(
	[property: DataTag] ResourceId Type,
	[property: DataTag] Dictionary<string, IExpression>? Arguments = null
) : IExpression;

public sealed class ConstructExpressionHandler : IExpressionHandler<ConstructExpression>
{
	public DataTag Handle(ScriptContext context, ConstructExpression request)
	{
		var type = context.Data.GetType(request.Type.ToString());
		if (type is null)
		{
			context.Logger.Error($"Could not construct unknown type [{request.Type}]");
			return new NullTag();
		}

		var compound = new CompoundTag().Set(context.Data.TypeDiscriminatorKey, request.Type.ToString());
		foreach (var (key, argument) in request.Arguments ?? [])
		{
			compound.Set(key, context.Evaluate(argument));
		}

		try
		{
			var instance = context.Data.FromTag(compound, type);
			return instance is null ? new NullTag() : new ObjectRefTag(instance);
		}
		catch (Exception ex)
		{
			context.Logger.Error($"Could not construct [{request.Type}]: {ex.Message}");
			return new NullTag();
		}
	}
}
