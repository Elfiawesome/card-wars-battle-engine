using CardWars.BattleEngine;
using CardWars.BattleEngineVanilla.Event;
using CardWars.BattleEngineVanilla.Input;

namespace CardWars.BattleEngineVanilla;

public class VanillaMod : IBattleEngineMod
{
	public string ModName => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry)
	{
		registry.InputHandlers.Register(new TestInputHandler());
		
		registry.EventHandlers.Register(new TestEventHandler());
	}
}
