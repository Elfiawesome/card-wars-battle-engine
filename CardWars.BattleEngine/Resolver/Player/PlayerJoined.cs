using CardWars.BattleEngine.Block.Entity;
using CardWars.BattleEngine.Block.Turn;
using CardWars.BattleEngine.Event;
using CardWars.BattleEngine.Event.Player;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Resolver.Player;

public class PlayerJoinedResolver(PlayerJoinedEvent evnt) : EventResolver<PlayerJoinedEvent>(evnt)
{
	public override void HandleStart()
	{
		var playerId = Event.PlayerId;

		var batch = Open();
		batch.Blocks.Add(new AddTurnOrderBlock(playerId));
		batch.Blocks.Add(new InstantiatePlayerBlock(playerId));

		// Check if this is the first player. Because we need to intialize the phase and allowed player inputs for them
		if (ServiceContainer?.State.CurrentPlayerId == null)
		{
			batch.Blocks.Add(new SetTurnIndexBlock(0, TurnPhase.Setup, true));
			batch.Blocks.Add(new AddAllowedPlayerInputsBlock(playerId));
		}


		var battlefieldId = new BattlefieldId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateBattlefieldBlock(battlefieldId));
		batch.Blocks.Add(new AttachBattlefieldToPlayerBlock(battlefieldId, playerId));

		var spellDeckId = new DeckId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateDeckBlock(spellDeckId));
		batch.Blocks.Add(new ModifyDeckTypeBlock(spellDeckId, DeckType.Spell));
		batch.Blocks.Add(new AttachDeckToPlayerBlock(spellDeckId, playerId, DeckType.Spell));

		var unitDeckId = new DeckId(Guid.NewGuid());
		batch.Blocks.Add(new InstantiateDeckBlock(unitDeckId));
		batch.Blocks.Add(new ModifyDeckTypeBlock(unitDeckId, DeckType.Unit));
		batch.Blocks.Add(new AttachDeckToPlayerBlock(unitDeckId, playerId, DeckType.Unit));

		var playerSetupEvent = new PlayerSetupEvent() { PlayerId = playerId };
		ServiceContainer?.EventService.Raise(playerSetupEvent);

		for (int i = 0; i < playerSetupEvent.UnitSlotCount; i++)
		{
			var unitSlotId = new UnitSlotId(Guid.NewGuid());
			batch.Blocks.Add(new InstantiateUnitSlotBlock(unitSlotId));
			batch.Blocks.Add(new AttachUnitSlotToBattlefieldBlock(unitSlotId, battlefieldId));
		}

		CommitResolved();
	}
}