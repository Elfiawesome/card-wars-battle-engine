using CardWars.BattleEngine.Event.Player;
using CardWars.BattleEngine.Event.Turn;
using CardWars.Core.Common.Dispatching;

namespace CardWars.BattleEngine.Event;

public class EventResolverDispatcher : RequestDispatcher<IServiceContainer, IEvent>
{
	public override void Register()
	{
		// Here we register all "end resolvers" for the events (if needed)
		RegisterHandler(new DrawCardEventHandler());
		RegisterHandler(new IntendPlayCardEventHandler());
		RegisterHandler(new PlayerJoinedEventHandler());
		RegisterHandler(new PlayerSetupEventHandler());

		RegisterHandler(new EndPhaseEventHandler());
		RegisterHandler(new RequestEndTurnEventHandler());
	}
}