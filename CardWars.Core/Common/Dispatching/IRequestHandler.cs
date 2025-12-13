namespace CardWars.Core.Common.Dispatching;

public interface IRequestHandler<in TRequest>
	where TRequest : IRequest
{
	public void Handle(TRequest request);
}

public interface IRequestHandler<in TContext, in TRequest>
	where TRequest : IRequest
	where TContext : notnull
{
	public void Handle(TContext context, TRequest request);
}

public interface IRequestHandler<in TContext, in TRequest, out TReturn>
	where TRequest : IRequest
	where TContext : notnull
	where TReturn : notnull
{
	public TReturn Handle(TContext context, TRequest request);
}

// public interface IRequestHandler<in TContext, in TRequest, out TReturn>
// 	where TRequest : IRequest
// 	where TContext : notnull
// {
// 	public TReturn? Handle(TContext context, TRequest request);
// }