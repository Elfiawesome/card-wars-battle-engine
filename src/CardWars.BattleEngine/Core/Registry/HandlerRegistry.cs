using CardWars.BattleEngine.Core.Request;

namespace CardWars.BattleEngine.Core.Registry;

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
			else { /* TODO: Bad request... */ }
		};
		base.Register(typeof(TRequest), action);
	}

	public void Execute<TRequest>(TContext context, TRequest request)
		where TRequest : IRequest
	{
		var requestType = request.GetType();
		var action = Get(requestType);
		if (action == null) { Console.WriteLine($"No handler found for {requestType}"); return; } /* TODO: Bad request*/
		action.Invoke(context, request);
	}
}