using CardWars.BattleEngine.Vanilla.Behaviour;
using CardWars.BattleEngine.Vanilla.Event;
using CardWars.BattleEngine.Vanilla.Input;
namespace CardWars.BattleEngine.Vanilla;

public class VanillaMod : IBattleEngineMod
{
	public string ModName => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry)
	{
		registry.InputHandlers.Register(new TestInputHandler());
		registry.InputHandlers.Register(new TestAdditionalInputHandler());
		
		registry.EventHandlers.Register(new TestEventHandler());
		
		registry.Behaviours.Register<SpecialStagedBehaviour>("special_staged_behaviour");
	}
}
