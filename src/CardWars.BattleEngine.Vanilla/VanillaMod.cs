using CardWars.BattleEngine.Vanilla.Block;
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

		registry.EventHandlers.Register(new EndTurnRequestEventHandler());
		registry.EventHandlers.Register(new EndPhaseEventHandler());
		registry.EventHandlers.Register(new EndTurnEventHandler());
		
		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
		registry.BlockHandlers.Register(new UpdateTurnStateBlockHandler());
	}
}
