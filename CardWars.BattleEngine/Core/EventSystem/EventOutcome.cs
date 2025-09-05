using CardWars.BattleEngine.Core.Resolvers;

namespace CardWars.BattleEngine.Core.EventSystem;

public class EventOutcome
{
	public List<Resolver> RaisedResolvers = [];
}