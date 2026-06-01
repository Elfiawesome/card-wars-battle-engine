using CardWars.BattleEngine;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using System;
using System.Linq;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();

	public Server.Server? IntegratedServer { get; private set; }
	public IConnection? Connection { get; private set; }
	public StorageManager Storage { get; private set; } = null!;

	public override void _Ready()
	{
		var clientDirPath = System.Environment.CurrentDirectory;
		var baseDirPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(clientDirPath, "..", ".."));
		var gamedataPath = System.IO.Path.Combine(baseDirPath, "gamedata");

		var provider = new LocalFileProvider();
		Storage = new StorageManager(gamedataPath, provider);

		var modDirs = Storage.GetModDirectories();

		foreach (var modDir in modDirs)
			Core.Logging.Logger.Info($"Mod directory: {modDir.FullPath}");

		ModLoader.ModLoader modLoader = new(modDirs);
		modLoader.Setup();
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry));

		StartIntegratedServer(modLoader);
	}

	private void StartIntegratedServer(ModLoader.ModLoader modLoader)
	{
		var sessionName = "session_1";
		IntegratedServer = new Server.Server(Storage, sessionName);

		var serverContent = modLoader.GetContentServer().ToList();
		var clientContent = modLoader.GetContentClient().ToList();

		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => IntegratedServer.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => IntegratedServer.LoadMod(m));

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
