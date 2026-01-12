using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Event.Player;
namespace CardWars.BattleEngine.Resolver.Player;

public class IntendPlayCardResolver(IntendPlayCardEvent evnt) : EventResolver<IntendPlayCardEvent>(evnt)
{
	public override void HandleStart()
	{
		if (ServiceContainer == null) { CommitResolved(); return; }
		
		var batch = Open();
		
		// We tell everyone this player's attempt to play this card is available or not
		// TODO
		
		CommitResolved();
	}
}