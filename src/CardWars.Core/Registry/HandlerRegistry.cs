using CardWars.Core.Logging;
using CardWars.Core.Request;

namespace CardWars.Core.Registry;

// HandlerRegistry is Core infrastructure with no natural per-session owner: Execute only has a
// TContext and an IRequest in reach, and TContext is not constrained to expose a logger, so a
// logger cannot be threaded generically through the request. Per the Phase 3 decision, it instead
// accepts an optional ILogger via its constructor so an owner that has one (e.g. a server/session)
// can inject it; otherwise it falls back to the immutable no-op NullLogger.Instance. This keeps
// Debug/Error visibility available where an owner wires a logger and holds no mutable static state
// (R1): the old static Log.Debug/Log.Error calls are gone.
public class HandlerRegistry<TContext>(ILogger? logger = null) : Registry<Type, Action<TContext, IRequest>>
	where TContext : notnull
{
	private readonly ILogger _logger = logger ?? NullLogger.Instance;

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
				_logger.Error($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
			}
		};
		base.Register(typeof(TRequest), action);
	}

	public void Execute<TRequest>(TContext context, TRequest request)
		where TRequest : IRequest
	{
		var requestType = request.GetType();
		_logger.Debug($"Running handler {request.GetType().Name}");
		var action = Get(requestType);
		if (action is null) { _logger.Error($"No handler found for {requestType}"); return; } /* TODO: Bad request*/
		action.Invoke(context, request);
	}
}

public class HandlerRegistry<TContext, TResult>(ILogger? logger = null) : Registry<Type, Func<TContext, IRequest, TResult>>
	where TContext : notnull
{
	private readonly ILogger _logger = logger ?? NullLogger.Instance;

	public void Register<TRequest>(IRequestHandler<TContext, TRequest, TResult> handler)
		where TRequest : IRequest
	{
		Func<TContext, IRequest, TResult> action = (context, request) =>
		{
			if (request is TRequest specificRequest)
			{
				return handler.Handle(context, specificRequest);
			}

			_logger.Error($"Handler for {typeof(TRequest)} received a request of type {request.GetType()}");
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
			_logger.Error($"No handler found for {requestType}");
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