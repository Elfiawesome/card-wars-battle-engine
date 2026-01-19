namespace CardWars.BattleEngine.Core.Request;

public interface IRequestHandler<in TContext, in TRequest>
	where TRequest : IRequest
	where TContext : notnull
{
	public void Handle(TContext context, TRequest request);
}