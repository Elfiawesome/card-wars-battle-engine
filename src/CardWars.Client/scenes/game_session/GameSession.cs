using System;
using CardWars.BattleEngine;
using CardWars.Server;
using Godot;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();

	public Server.Server Server = new();

	public override void _Ready()
	{

		ModLoader.ModLoader modLoader = new(@"C:\Users\Elfiyan\Documents\Projects\card-wars-battle-engine\mods");

		modLoader.Setup();
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry));
		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => Server.LoadMod(m));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => Server.LoadMod(m));
	}
}
