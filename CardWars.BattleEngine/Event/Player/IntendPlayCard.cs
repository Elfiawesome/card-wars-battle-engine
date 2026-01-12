using CardWars.BattleEngine.Resolver.Player;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Event.Player;

public class IntendPlayCardEvent : IEvent
{
	public PlayerId PlayerId;
	public int PlayingCardHandPos;
	public TargetPlay TargetPlay;
	public IStateId? TargetId = null;

	public bool IntendUnavailable = false;
	public string IntendUnavailableReason = ""; // We can replace this with something more complex which highlights what is causing this intent to play the card to be unavailable
}

public class IntendPlayCardEventHandler : IEventHandler<IntendPlayCardEvent>
{
	public void Handle(IServiceContainer serviceContainer, IntendPlayCardEvent request)
	{
		serviceContainer.Resolver.QueueResolver(new IntendPlayCardResolver(request));
	}
}