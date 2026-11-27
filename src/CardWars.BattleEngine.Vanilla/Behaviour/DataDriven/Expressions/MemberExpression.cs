using CardWars.BattleEngine.Behaviour.DataDriven.Expressions;
using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Vanilla.Behaviour.DataDriven.Expressions;

[DataTagType("script/expression/member")]
public record struct MemberExpression(
	[property: DataTag] IExpression Target,
	[property: DataTag] string Member
) : IExpression;

public sealed class MemberExpressionHandler : IExpressionHandler<MemberExpression>
{
	public DataTag Handle(ScriptContext context, MemberExpression request)
	{
		var target = context.Evaluate(request.Target);
		var targetObject = target is ObjectRefTag objectRef ? objectRef.Value : target;

		if (targetObject is NullTag)
		{
			context.Logger.Warn($"Cannot read member [{request.Member}] from null");
			return new NullTag();
		}

		if (context.DataTags.TryGetMember(targetObject, request.Member, out var value))
			return value ?? new NullTag();

		context.Logger.Warn($"Member [{request.Member}] was not found on [{targetObject.GetType().Name}]");
		return new NullTag();
	}
}
