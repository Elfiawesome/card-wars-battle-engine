using CardWars.BattleEngine.Behaviour.DataDriven.Runtime;
using CardWars.Core.Data.Tags;
using CardWars.Core.Request;

namespace CardWars.BattleEngine.Behaviour.DataDriven.Expressions;

public interface IExpressionHandler<TExpression> : IRequestHandler<ScriptContext, TExpression, DataTag>
	where TExpression : IExpression;
