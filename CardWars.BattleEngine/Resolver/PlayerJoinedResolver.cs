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
		BlockBatch batch = new([
			new AddTurnOrderBlock(PlayerId),
			new InstantiatePlayerBlock(PlayerId),
		]);

		// Set turn order if its the first player
		if (engine.TurnService.CurrentPlayerId == null)
		{
			batch.Blocks.Add(new SetTurnIndexBlock(0, TurnService.PhaseType.Setup, true));
			batch.Blocks.Add(new AddAllowedPlayerInputsBlock(PlayerId, true));
		}

		var battlefieldId = new BattlefieldId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateBattlefieldBlock(battlefieldId));
		batch.Blocks.Add(new AttachBattlefieldToPlayerBlock(battlefieldId, PlayerId));

		// TODO PlayerJoinedResolver will need the battlefield prefab (which will be an empty with unit slots)
		// For now lets just instantiate 4 of them manually
		for (int i = 0; i < 4; i++)
		{
			var unitSlotId = new UnitSlotId(Guid.NewGuid());
			batch.Blocks.Add(new InstantiateUnitSlotBlock(unitSlotId));
			batch.Blocks.Add(new AttachUnitSlotToBattlefieldBlock(unitSlotId, battlefieldId));
		}
		AddBlockBatch(batch);
		CommitResolved();
	}
}