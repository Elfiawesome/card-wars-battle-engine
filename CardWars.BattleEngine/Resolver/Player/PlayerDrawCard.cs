using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Resolver.Player;

public class PlayerDrawCard : Resolver
{
	public PlayerId PlayerId;

	public override void HandleStart()
	{
		if (ServiceContainer == null) { CommitResolved(); return; }
		var batch = Open();

		// Get card
		if (ServiceContainer.State.Players.TryGetValue(PlayerId, out var player))
		{
			
		}
	}
}
