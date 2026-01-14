using CardWars.BattleEngine.Resolver;

namespace CardWars.BattleEngine.Feature.PlayerSetup;

public class PlayerSetupResolver(PlayerSetupEvent evnt) : EventResolver<PlayerSetupEvent>(evnt)
{
	public override void HandleStart()
	{
		// Uh theres nothing i guess...
		// Technically we can remove this and remove it from the dispatcher
		CommitResolved();
	}
}