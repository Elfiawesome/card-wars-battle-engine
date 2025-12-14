using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Block.Turn;
using CardWars.BattleEngine.Event.Turn;
using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Resolver.Player;

public class PlayerJoinedResolver : Resolver
{
	public PlayerId PlayerId;

	public override void HandleStart()
	{
		var batch = Open();
		batch.Blocks.Add(new AddTurnOrderBlock(PlayerId));
		batch.Blocks.Add(new InstantiatePlayerBlock(PlayerId));

		// Check if this is the first player. Because we need to intialize the phase and allowed player inputs for them
		if (ServiceContainer?.State.CurrentPlayerId == null)
		{
			batch.Blocks.Add(new SetTurnIndexBlock(0, TurnPhase.Setup, true));
			batch.Blocks.Add(new AddAllowedPlayerInputsBlock(PlayerId));
		}


		var battlefieldId = new BattlefieldId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateBattlefieldBlock(battlefieldId));
		batch.Blocks.Add(new AttachBattlefieldToPlayerBlock(battlefieldId, PlayerId));

		var spellDeckId = new DeckId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateDeckBlock(spellDeckId));
		batch.Blocks.Add(new ModifyDeckTypeBlock(spellDeckId, DeckType.Spell));
		batch.Blocks.Add(new AttachDeckToPlayerBlock(spellDeckId, PlayerId, DeckType.Spell));

		var unitDeckId = new DeckId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateDeckBlock(unitDeckId));
		batch.Blocks.Add(new ModifyDeckTypeBlock(unitDeckId, DeckType.Unit));
		batch.Blocks.Add(new AttachDeckToPlayerBlock(unitDeckId, PlayerId, DeckType.Unit));

		// We only raise the event here beacuse when we add our heroes, there will be abilities that 
		// can +/- the unit slots on the battlefield
		ServiceContainer?.EventService.Raise(new PlayerJoinedEvent());

		for (int i = 0; i < 4; i++)
		{
			var unitSlotId = new UnitSlotId(Guid.NewGuid());
			batch.Blocks.Add(new InstantiateUnitSlotBlock(unitSlotId));
			batch.Blocks.Add(new AttachUnitSlotToBattlefieldBlock(unitSlotId, battlefieldId));
		}
		
		CommitResolved();
	}
}