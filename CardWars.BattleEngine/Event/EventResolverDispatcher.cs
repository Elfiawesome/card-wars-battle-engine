using CardWars.BattleEngine.Feature.DrawCard;
using CardWars.BattleEngine.Feature.EndTurn;
using CardWars.BattleEngine.Feature.IntendPlayCard;
using CardWars.BattleEngine.Feature.PlayerJoined;
using CardWars.BattleEngine.Feature.PlayerSetup;
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