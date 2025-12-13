using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Resolver;

public class ResolverService(IServiceContainer container) : Service(container)
{
	private readonly List<Resolver> resolverQueue = [];

	public void QueueResolver(Resolver resolver)
	{
		resolver.ServiceContainer = ServiceContainer;
		resolver.HandleStart();
		if (!resolver.IsResolved)
		{
			resolverQueue.Add(resolver);
		}
		else
		{
			ProcessResolvers();
		}
	}

	public void HandleInput(PlayerId playerId, IInput input)
	{
		if (resolverQueue.Count < 1) { return; }
		var currentResolver = resolverQueue[0];
		currentResolver.HandleInput(playerId, input);
		if (!currentResolver.IsResolved)
		{
			resolverQueue.RemoveAt(0);
			ProcessResolvers();
		}
	}

	private void ProcessResolvers()
	{
		while (true)
		{
			if (resolverQueue.Count < 1) { break; }
			var currentResolver = resolverQueue[0];
			currentResolver.HandleStart();
			if (!currentResolver.IsResolved)
			{
				resolverQueue.RemoveAt(0);
			}
			else
			{
				break;
			}
		}
	}
}