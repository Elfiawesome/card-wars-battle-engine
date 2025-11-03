namespace CardWars.Core.Common.Dispatching;

public abstract class RequestDispatcher<TContext, TRequest, TReturn>
	where TRequest : IRequest
{
	private readonly Dictionary<Type, Func<TContext, TRequest, TReturn?>> _handlers = [];

	public abstract void Register();

	public TReturn? Handle(TContext context, TRequest baseRequest)
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

		// Create a lambda that performs the type-safe cast and calls the specific handler.
		_handlers[requestType] = (context, request) =>
			handler.Handle(context, (TSpecificRequest)request);
	}
}