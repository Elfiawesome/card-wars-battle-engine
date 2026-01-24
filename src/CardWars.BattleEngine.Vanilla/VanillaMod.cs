using CardWars.BattleEngine.Vanilla.Block;
using CardWars.BattleEngine.Vanilla.Input;

namespace CardWars.BattleEngine.Vanilla;

public class VanillaMod : IBattleEngineMod
{
	public string ModName => "Vanilla Mod";
	public string Version => "0.0.0";

	public void OnLoad(BattleEngineRegistry registry)
	{
		registry.InputHandlers.Register(new PlayerJoinedInputHandler());

		registry.BlockHandlers.Register(new InstantiatePlayerBlockHandler());
	}
}
