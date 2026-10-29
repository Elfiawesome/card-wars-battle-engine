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
			else
			{
				Log.Error($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
			}
		};
		base.Register(typeof(TRequest), action);
	}

	public void Execute<TRequest>(TContext context, TRequest request)
		where TRequest : IRequest
	{
		var requestType = request.GetType();
		Log.Debug($"Running handler {request.GetType().Name}");
		var action = Get(requestType);
		if (action is null) { Log.Error($"No handler found for {requestType}"); return; } /* TODO: Bad request*/
		action.Invoke(context, request);
	}
}

public class HandlerRegistry<TContext, TResult> : Registry<Type, Func<TContext, IRequest, TResult>>
	where TContext : notnull
{
	public void Register<TRequest>(IRequestHandler<TContext, TRequest, TResult> handler)
		where TRequest : IRequest
	{
		Func<TContext, IRequest, TResult> action = (context, request) =>
		{
			if (request is TRequest specificRequest)
			{
				return handler.Handle(context, specificRequest);
			}

			Log.Error($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
			throw new InvalidOperationException($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
		};
		base.Register(typeof(TRequest), action);
	}

	public bool TryExecute<TRequest>(TContext context, TRequest request, out TResult? result)
		where TRequest : IRequest
	{
		var requestType = request.GetType();
		var action = Get(requestType);
		if (action is null)
		{
			Log.Error($"No handler found for {requestType}");
			result = default;
			return false;
		}

		result = action.Invoke(context, request);
		return true;
	}

	public TResult? Execute<TRequest>(TContext context, TRequest request)
		where TRequest : IRequest
		=> TryExecute(context, request, out var result) ? result : default;
}