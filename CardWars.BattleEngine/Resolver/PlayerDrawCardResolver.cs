using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Resolver;

public class PlayerDrawCardResolver(PlayerId playerId, DeckId deckId) : ResolverBase
{
	public PlayerId PlayerId = playerId;
	public DeckId DeckId = deckId;
	
	public override void HandleStart(BattleEngine engine)
	{
		CommitResolved();
	}
}