namespace CardWars.Core.Common.Dispatching;

public abstract class RequestHandlerBase
{
	public abstract void Register();
}


public abstract class RequestDispatcher<TRequest> : RequestHandlerBase
	where TRequest : IRequest
{
	private readonly Dictionary<Type, Action<TRequest>> _handlers = [];

	public void Handle(TRequest baseRequest)
	{
		var requestType = baseRequest.GetType();
		if (_handlers.TryGetValue(requestType, out var handler))
		{
			handler.Invoke(baseRequest);
		}
		else
		{
			throw new InvalidOperationException($"No handler registered for command type: {requestType.Name}");
		}
	}

	public void RegisterHandler<TSpecificRequest>(IRequestHandler<TSpecificRequest> handler)
		where TSpecificRequest : TRequest
	{
		var requestType = typeof(TSpecificRequest);
		if (_handlers.ContainsKey(requestType))
		{
			throw new InvalidOperationException($"A handler for {requestType.Name} is already registered.");
		}

		_handlers[requestType] = (request) =>
		{
			Console.WriteLine($"[{GetType().Name}] --> {request}");
			handler.Handle((TSpecificRequest)request);
		};
	}
}

public abstract class RequestDispatcher<TContext, TRequest> : RequestHandlerBase
	where TRequest : IRequest
	where TContext : notnull
{
	private readonly Dictionary<Type, Action<TContext, TRequest>> _handlers = [];

	public void Handle(TContext context, TRequest baseRequest)
	{
		var requestType = baseRequest.GetType();
		if (_handlers.TryGetValue(requestType, out var handler))
		{
			handler.Invoke(context, baseRequest);
		}
		else
		{
			throw new InvalidOperationException($"No handler registered for command type: {requestType.Name}");
		}
	}

	public void RegisterHandler<TSpecificRequest>(IRequestHandler<TContext, TSpecificRequest> handler)
		where TSpecificRequest : TRequest
	{
		var requestType = typeof(TSpecificRequest);
		if (_handlers.ContainsKey(requestType))
		{
			throw new InvalidOperationException($"A handler for {requestType.Name} is already registered.");
		}

		_handlers[requestType] = (context, request) =>
		{
			Console.WriteLine($"[{GetType().Name}] --> {request}");
			handler.Handle(context, (TSpecificRequest)request);
		};
	}
}

public abstract class RequestDispatcher<TContext, TRequest, TReturn> : RequestHandlerBase
	where TRequest : IRequest
	where TContext : notnull
	where TReturn : notnull
{
	private readonly Dictionary<Type, Func<TContext, TRequest, TReturn>> _handlers = [];

	public TReturn Handle(TContext context, TRequest baseRequest)
	{
		var requestType = baseRequest.GetType();
		if (_handlers.TryGetValue(requestType, out var handler))
		{
			return handler.Invoke(context, baseRequest);
		}
		throw new InvalidOperationException($"No handler registered for command type: {requestType.Name}");
	}

	public void RegisterHandler<TSpecificRequest>(IRequestHandler<TContext, TSpecificRequest, TReturn> handler)
		where TSpecificRequest : TRequest
	{
		var requestType = typeof(TSpecificRequest);
		if (_handlers.ContainsKey(requestType))
		{
			throw new InvalidOperationException($"A handler for {requestType.Name} is already registered.");
		}

		_handlers[requestType] = (context, request) =>
		{
			Console.WriteLine($"[{GetType().Name}] --> {request}");
			return handler.Handle(context, (TSpecificRequest)request);
		};
	}
}