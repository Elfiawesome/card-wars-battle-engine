using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Global;
using CardWars.Core.Data.Mapping;
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
	public DataTag Handle(ScriptRuntimeContext context, ConstructExpression request)
	{
		var type = DataTagTypeRegistry.GetType(request.Type.ToString());
		if (type == null)
		{
			Log.Error($"Could not construct unknown type [{request.Type}]");
			return new NullTag();
		}

		var compound = new CompoundTag().Set(DataTagMapper.TypeDiscriminatorKey, request.Type.ToString());
		foreach (var (key, argument) in request.Arguments ?? [])
		{
			compound.Set(key, context.Evaluate(argument));
		}

		try
		{
			var instance = DataTagMapper.FromTag(compound, type);
			return instance == null ? new NullTag() : new ObjectRefTag(instance);
		}
		catch (Exception ex)
		{
			Log.Error($"Could not construct [{request.Type}]: {ex.Message}");
			return new NullTag();
		}
	}
}
