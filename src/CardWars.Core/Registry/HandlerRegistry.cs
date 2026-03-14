using CardWars.Core.Logging;
using CardWars.Core.Request;

namespace CardWars.Core.Registry;

public class HandlerRegistry<TContext> : Registry<Type, Action<TContext, IRequest>>
	where TContext : notnull
{
	public void Register<TRequest>(IRequestHandler<TContext, TRequest> handler)
		where TRequest : IRequest
	{
		Action<TContext, IRequest> action = (context, request) =>
		{
			if (request is TRequest specificReqeust)
			{
				handler.Handle(context, specificReqeust);
			}
			else { 
				Logger.Error($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
			}
		};
		base.Register(typeof(TRequest), action);
	}

	public void Execute<TRequest>(TContext context, TRequest request)
		where TRequest : IRequest
	{
		var requestType = request.GetType();
		Logger.Debug($"Running handler {request.GetType().Name}");
		var action = Get(requestType);
		if (action == null) { Logger.Error($"No handler found for {requestType}"); return; } /* TODO: Bad request*/
		action.Invoke(context, request);
	}
}