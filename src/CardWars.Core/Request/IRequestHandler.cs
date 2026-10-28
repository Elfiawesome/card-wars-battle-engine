namespace CardWars.Core.Request;

public interface IRequestHandler<in TContext, in TRequest>
	where TRequest : IRequest
	where TContext : notnull
{
	public void Handle(TContext context, TRequest request);
}

public interface IRequestHandler<in TContext, in TRequest, out TResult>
	where TRequest : IRequest
	where TContext : notnull
{
	public TResult Handle(TContext context, TRequest request);
}