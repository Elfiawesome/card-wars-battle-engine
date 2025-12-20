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
		batch.AddBlock(new AddTurnOrderBlock(playerId));
		batch.AddBlock(new InstantiatePlayerBlock(playerId));

		// Check if this is the first player. Because we need to intialize the phase and allowed player inputs for them
		if (ServiceContainer?.State.CurrentPlayerId == null)
		{
			batch.AddBlock(new SetTurnIndexBlock(0, TurnPhase.Setup, true));
			batch.AddBlock(new AddAllowedPlayerInputsBlock(playerId));
		}


		var battlefieldId = new BattlefieldId(Guid.NewGuid());
		batch.AddBlock(new InstantiateBattlefieldBlock(battlefieldId));
		batch.AddBlock(new AttachBattlefieldToPlayerBlock(battlefieldId, playerId));

		var spellDeckId = new DeckId(Guid.NewGuid());
		batch.AddBlock(new InstantiateDeckBlock(spellDeckId));
		batch.AddBlock(new ModifyDeckTypeBlock(spellDeckId, DeckType.Spell));
		batch.AddBlock(new AttachDeckToPlayerBlock(spellDeckId, playerId, DeckType.Spell));

		var unitDeckId = new DeckId(Guid.NewGuid());
		batch.AddBlock(new InstantiateDeckBlock(unitDeckId));
		batch.AddBlock(new ModifyDeckTypeBlock(unitDeckId, DeckType.Unit));
		batch.AddBlock(new AttachDeckToPlayerBlock(unitDeckId, playerId, DeckType.Unit));

		var playerSetupEvent = new PlayerSetupEvent() { PlayerId = playerId };
		ServiceContainer?.EventService.Raise(playerSetupEvent);

		for (int i = 0; i < playerSetupEvent.UnitSlotCount; i++)
		{
			var unitSlotId = new UnitSlotId(Guid.NewGuid());
			batch.AddBlock(new InstantiateUnitSlotBlock(unitSlotId));
			batch.AddBlock(new AttachUnitSlotToBattlefieldBlock(unitSlotId, battlefieldId));
		}

		CommitResolved();
	}
}