using CardWars.BattleEngine;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using System.Linq;
using System.IO;
using CardWars.Core.FileSystem;
using System;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();

	public Server.Server? IntegratedServer { get; private set; }
	public IConnection? Connection { get; private set; }

	public override void _Ready()
	{
		var clientDirPath = new DirectoryInfo(System.Environment.CurrentDirectory);
		var baseDirPath = clientDirPath.Parent?.Parent?.FullName ?? throw new Exception("Could not determine base directory path");

		var prnt = Core.Logging.Logger.Info;
		IFileSystem globalFs = new LocalFileSystem(baseDirPath);
		IPathAddr savesDir = globalFs.GetPath("saves");
		IPathAddr globalModsDir = globalFs.GetPath("mods");
		globalModsDir.Walk().ToList().ForEach(p => prnt($"{p.relativePath.ToPath()}"));

		// string sessionId = "session_1";
		// IFileSystem sessionFs = new LocalFileSystem(savesDir.Combine(sessionId).Path);
		// IPathAddr sessionModsDir = sessionFs.GetPath("mods");

		// Creating our folder layouts using abstractions if missing
		// savesDir.CreateDirectory();
		// globalModsDir.CreateDirectory();
		// sessionFs.Root.CreateDirectory();
		// sessionModsDir.CreateDirectory();

		// ModLoader.ModLoader modLoader = new([globalModsDir, sessionModsDir]);
		// modLoader.Setup();
		// modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry));

		// StartIntegratedServer(modLoader, sessionFs);
	}

	private void StartIntegratedServer(ModLoader.ModLoader modLoader, IFileSystem sessionFs)
	{
		IntegratedServer = new Server.Server(sessionFs);

		var serverContent = modLoader.GetContentServer().ToList();
		var clientContent = modLoader.GetContentClient().ToList();

		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => IntegratedServer.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => IntegratedServer.LoadMod(m));

		var localListener = new LocalListener();

		// Note: To make this LAN compatible later, you would just add:
		// var tcpListener = new TcpGameListener();
		// IntegratedServer.Start(localListener, tcpListener);

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
