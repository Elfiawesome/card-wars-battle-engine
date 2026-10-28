using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/member")]
public record struct MemberExpression(
	[property: DataTag] IExpression Target,
	[property: DataTag] string Member
) : IExpression;

public sealed class MemberExpressionHandler : IExpressionHandler<MemberExpression>
{
	public DataTag Handle(ScriptRuntimeContext context, MemberExpression request)
	{
		var target = context.Evaluate(request.Target);
		var targetObject = target is ObjectRefTag objectRef ? objectRef.Value : target;

		if (targetObject is NullTag)
		{
			Log.Warn($"Cannot read member [{request.Member}] from null");
			return new NullTag();
		}

		if (context.DataDriven.ObjectAccessors.TryGetMember(targetObject, request.Member, out var value))
			return value ?? new NullTag();

		Log.Warn($"Member [{request.Member}] was not found on [{targetObject.GetType().Name}]");
		return new NullTag();
	}
}
