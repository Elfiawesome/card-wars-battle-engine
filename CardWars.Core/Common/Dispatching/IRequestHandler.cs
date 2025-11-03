namespace CardWars.Core.Common.Dispatching;

public interface IRequestHandler<in TContext, in TRequest, out TReturn>
	where TRequest : IRequest
{
	public TReturn? Handle(TContext? context, TRequest request);
}