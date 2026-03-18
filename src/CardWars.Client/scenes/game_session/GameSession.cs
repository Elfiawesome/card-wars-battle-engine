using CardWars.BattleEngine;
using CardWars.Server;
using Godot;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();
	public BattleEngineRegistry BattleEngineRegistry { get; init; } = new();
	public ServerRegistry ServerRegistry { get; init; } = new();

	public override void _Ready()
	{

		ModLoader.ModLoader modLoader = new(@"C:\Users\Elfiyan\Documents\Projects\card-wars-battle-engine\mods");

		modLoader.Setup();
		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => m.OnLoad(BattleEngineRegistry));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => m.OnLoad(ServerRegistry));
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry));
	}
}
