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
			if (request is TRequest specificRequest)
			{
				handler.Handle(context, specificRequest);
				return;
			}

			throw new InvalidOperationException($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
		};
		base.Register(typeof(TRequest), action);
	}

	public void Execute<TRequest>(TContext context, TRequest request)
		where TRequest : IRequest
	{
		var action = Get(request.GetType());
		if (action is null) return;
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
				return handler.Handle(context, specificRequest);

			throw new InvalidOperationException($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
		};
		base.Register(typeof(TRequest), action);
	}

	public bool TryExecute<TRequest>(TContext context, TRequest request, out TResult? result)
		where TRequest : IRequest
	{
		var action = Get(request.GetType());
		if (action is null)
		{
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
