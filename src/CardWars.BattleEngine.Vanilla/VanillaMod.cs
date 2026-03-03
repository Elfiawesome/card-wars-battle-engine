using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.Data;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Features;

namespace CardWars.BattleEngine.Vanilla;

public class VanillaMod : IBattleEngineMod
{
	public string ModName => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry)
	{
		// --- Input Handlers ---
		registry.InputHandlers.Register(new PlayerJoinedRequestInputHandler());
		registry.InputHandlers.Register(new EndTurnRequestInputHandler());
		registry.InputHandlers.Register(new DrawCardRequestInputHandler());

		// --- Event Handlers ---
		registry.EventHandlers.Register(new PlayerJoinedEventHandler());
		registry.EventHandlers.Register(new EndTurnRequestEventHandler());
		registry.EventHandlers.Register(new EndPhaseEventHandler());
		registry.EventHandlers.Register(new EndTurnEventHandler());
		registry.EventHandlers.Register(new DrawCardRequestEventHandler());

		// --- Block Handlers ---
		registry.BlockHandlers.Register(new AttachCardToDeckBlockHandler());
		registry.BlockHandlers.Register(new AttachCardToPlayerBlockHandler());
		registry.BlockHandlers.Register(new AttachDeckToPlayerBlockHandler());
		registry.BlockHandlers.Register(new DetachCardFromDeckBlockHandler());
		registry.BlockHandlers.Register(new DetachCardFromPlayerBlockHandler());
		registry.BlockHandlers.Register(new InstantiateCardBlockHandler());
		registry.BlockHandlers.Register(new InstantiateDeckBlockHandler());
		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
		registry.BlockHandlers.Register(new SetCardDataBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());

		// --- Card Definitions ---
		RegisterCardDefinitions(registry);
	}


	private void RegisterCardDefinitions(BattleEngineRegistry registry)
	{
		registry.CardDefinitions.Register(ResourceId.Vanilla("cards/john"), new CompoundTag()
		.Set("name", "John")
		.Set("pt", 2)
		.Set("hp", 6)
		.Set("atk", 4)
		.Set("abilities", new ListTag()
			.Add(new CompoundTag()
				.Set("description", "A special ability for this unit")
				.Set("behaviour", new CompoundTag()
					.Set("resource", "cardwars:some_behaviour")))));

		registry.CardDefinitions.Register(ResourceId.Vanilla("cards/elbert"), new CompoundTag()
			.Set("name", "Elbert")
			.Set("pt", 1)
			.Set("hp", 2)
			.Set("atk", 3));

		registry.CardDefinitions.Register(ResourceId.Vanilla("cards/nicholas"), new CompoundTag()
			.Set("name", "Nicholas")
			.Set("pt", 5)
			.Set("hp", 10)
			.Set("atk", 1));
	}
}
