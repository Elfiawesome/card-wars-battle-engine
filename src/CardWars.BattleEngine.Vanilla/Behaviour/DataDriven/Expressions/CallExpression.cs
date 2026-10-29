using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/call")]
public record struct CallExpression(
	[property: DataTag] ResourceId Function,
	[property: DataTag] Dictionary<string, IExpression>? Arguments = null
) : IExpression;

public sealed class CallExpressionHandler : IExpressionHandler<CallExpression>
{
	public DataTag Handle(ScriptContext context, CallExpression request)
	{
		var function = context.DataDriven.Functions.Get(request.Function);
		if (function == null)
		{
			Log.Error($"Unknown function [{request.Function}]");
			return new NullTag();
		}

		var arguments = new List<DataTag>();
		foreach (var (_, argument) in request.Arguments ?? [])
		{
			arguments.Add(context.Evaluate(argument));
		}

		return function.Invoke(context, arguments);
	}
}
