using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Feature.IntendPlayCard;

public class IntendPlayCardEvent : IEvent
{
	public PlayerId PlayerId;
	public required ICardId cardId;
	public TargetPlay TargetPlay;
	public IStateId? TargetId = null;

	public bool IntendUnavailable = false;
	public string IntendUnavailableReason = "";
}

public class IntendPlayCardEventHandler : IEventHandler<IntendPlayCardEvent>
{
	public void Handle(IServiceContainer serviceContainer, IntendPlayCardEvent request)
	{
		// serviceContainer.Resolver.QueueResolver(new IntendPlayCardResolver(request));
	}
}