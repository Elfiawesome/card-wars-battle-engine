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
		registry.InputHandlers.Register(new PlayerJoinedRequestInputHandler());
		registry.InputHandlers.Register(new EndTurnRequestInputHandler());
		
		registry.EventHandlers.Register(new PlayerJoinedEventHandler());
		registry.EventHandlers.Register(new EndTurnRequestEventHandler());
		registry.EventHandlers.Register(new EndPhaseEventHandler());
		registry.EventHandlers.Register(new EndTurnEventHandler());

		// registry.Entities.Register(ResourceId.Vanilla("battlefield"), id => new Battlefield(id));
		// registry.Entities.Register(ResourceId.Vanilla("player"), id => new Player(id));
		// registry.Entities.Register(ResourceId.Vanilla("deck"), id => new Deck(id));
		// registry.Entities.Register(ResourceId.Vanilla("unit_slot"), id => new UnitSlot(id));
		// registry.Entities.Register(ResourceId.Vanilla("card"), id => new GenericCard(id));
		
		registry.BlockHandlers.Register(new AttachDeckToPlayerHandler());
		registry.BlockHandlers.Register(new InstantiateDeckBlockHandler());
		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());
	}
}
