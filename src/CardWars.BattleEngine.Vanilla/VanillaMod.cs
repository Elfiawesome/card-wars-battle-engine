using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Entity;
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

		// --- Event Handlers ---
		registry.EventHandlers.Register(new PlayerJoinedEventHandler());
		registry.EventHandlers.Register(new EndTurnRequestEventHandler());
		registry.EventHandlers.Register(new EndPhaseEventHandler());
		registry.EventHandlers.Register(new EndTurnEventHandler());

		// --- Block Handlers ---
		registry.BlockHandlers.Register(new AttachDeckToPlayerHandler());
		registry.BlockHandlers.Register(new AttachDeckToPlayerBlockHandler());
		registry.BlockHandlers.Register(new InstantiateDeckBlockHandler());
		registry.BlockHandlers.Register(new InstantiateCardBlockHandler());
		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());

		// --- Card Definitions ---
		RegisterCardDefinitions(registry);
	}


	private void RegisterCardDefinitions(BattleEngineRegistry registry)
	{
		registry.CardContent.Register(ResourceId.Vanilla("cards/john"), new()
		{
			Name = "John",
			Pt = 2,
			Hp = 6,
			Atk = 4,
			Abilities = [
				new()
				{
					Description = "A special ability for this unit",
					Behaviour = new() { }
				}
			]
		});
		registry.CardContent.Register(ResourceId.Vanilla("cards/elbert"), new()
		{
			Name = "Elbert",
			Pt = 1,
			Hp = 2,
			Atk = 3
		});
		registry.CardContent.Register(ResourceId.Vanilla("cards/nicholas"), new()
		{
			Name = "Nicholas",
			Pt = 5,
			Hp = 10,
			Atk = 1
		});
	}
}
