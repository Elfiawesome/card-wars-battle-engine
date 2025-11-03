using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Block.Turn;
using CardWars.BattleEngine.Entity;

namespace CardWars.BattleEngine.Resolver;

public class PlayerJoinedResolver(PlayerId playerId) : ResolverBase
{
	public PlayerId PlayerId = playerId;
	
	public override void HandleStart(BattleEngine engine)
	{
		// TODO Implement a way to instantiate all the battlefields.
		// TODO PlayerJoinedResolver will probably need a context object fed into it to figure out the
		// heroes, unit deck, spell deck, etc
		BlockBatch batch = new([
			new AddTurnOrderBlock(PlayerId),
			new InstantiatePlayerBlock(PlayerId)
		]);
		
		if (engine.TurnService.CurrentPlayerId == null)
		{
			batch.Blocks.Add(new SetTurnIndexBlock(0, TurnService.PhaseType.Setup, true));
			batch.Blocks.Add(new AddAllowedPlayerInputsBlock(PlayerId, true));
		}
		AddBlockBatch(batch);
		CommitResolved();
	}
}