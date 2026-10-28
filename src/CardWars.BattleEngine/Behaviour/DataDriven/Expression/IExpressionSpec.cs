using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;
using CardWars.Core.Request;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Expression;

public class ScriptRuntimeContext
{
	public Dictionary<ResourceId, DataTag> Variables = [];
}

public interface IExpressionSpec : IRequest;

public interface IExpressionSpecHandler<TExpression> : IRequestHandler<ScriptRuntimeContext, TExpression>
	where TExpression : IExpressionSpec;


[DataTagType()]
public record struct GivenValueExpressionSpec(
	[property: DataTag] DataTag Value
) : IExpressionSpec;

[DataTagType()] public record struct AddExpressionSpec([property: DataTag] IExpressionSpec Value1, [property: DataTag] IExpressionSpec Value2) : IExpressionSpec;
[DataTagType()] public record struct MinusExpressionSpec([property: DataTag] IExpressionSpec Value1, [property: DataTag] IExpressionSpec Value2) : IExpressionSpec;
[DataTagType()] public record struct MultiplyExpressionSpec([property: DataTag] IExpressionSpec Value1, [property: DataTag] IExpressionSpec Value2) : IExpressionSpec;
[DataTagType()] public record struct DivideExpressionSpec([property: DataTag] IExpressionSpec Value1, [property: DataTag] IExpressionSpec Value2) : IExpressionSpec;

[DataTagType()] public record struct RetrieveVariable(
	[property: DataTag] DataTag Name,
	[property: DataTag] DataTag Path
);