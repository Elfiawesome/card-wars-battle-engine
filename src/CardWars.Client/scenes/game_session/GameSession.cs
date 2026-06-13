using CardWars.BattleEngine;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();

	public Server.Server? IntegratedServer { get; private set; }
	public IConnection? Connection { get; private set; }
	public StorageManager Storage { get; private set; } = null!;

	// Temporary settings
	public string ConnectingUsername = "Elfiawesome";

	public override void _Ready()
	{		
		// Setup storage & providers
		var provider = new LocalFileProvider();
		var clientDir = System.Environment.CurrentDirectory;
		var projectRoot = provider.GetFullPath(provider.Combine(provider.Combine(clientDir, ".."), ".."));
		var gamedataPath = provider.Combine(projectRoot, "gamedata");
		Storage = new StorageManager(gamedataPath, provider);
		
		// Start!
		StartIntegratedServer();
	}

	private void StartIntegratedServer()
	{
		var sessionName = "session_1";
		IntegratedServer = new Server.Server(Storage, sessionName);
		
		var modDirs = Storage.AllModDirectories;
		ModLoader.ModLoader modLoader = new(modDirs);
		modLoader.Setup();

		var clientContent = modLoader.GetContentClient().ToList();
		var serverContent = modLoader.GetContentServer().ToList();

		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => IntegratedServer.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => IntegratedServer.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry, clientContent));

		var localListener = new LocalListener();
		IntegratedServer.Start(localListener);
		Connection = localListener.ConnectClient();
	}

	public override void _Process(double delta)
	{
		if (Connection != null && Connection.IsConnected)
		{
			while (Connection.TryReceive(out var packet))
			{
				if (packet != null)
				{
					HandleIncomingPacket(packet);
				}
			}
		}
	}

	private void HandleIncomingPacket(IPacket packet)
	{
		Core.Logging.Logger.Debug($"Client received packet from server: {packet.GetType().Name}");
		ClientRegistry.PacketHandlers.Execute(new PacketContextClient() { Session = this }, packet);
	}

	public override void _ExitTree()
	{
		Connection?.Disconnect();
		IntegratedServer?.Stop();
	}
}
