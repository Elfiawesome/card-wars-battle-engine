using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CardWars.BattleEngine;
using CardWars.BattleEngine.Input;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();
	public BattleEngineRegistry BattleEngineRegistry { get; init; } = new();

	public Server.Server? IntegratedServer { get; private set; }
	public IConnection? Connection { get; private set; }
	public StorageManager Storage { get; private set; } = null!;

	public string ConnectingUsername = "";

	public Action? OnProcess { get; set; }
	public Action<IInput>? OnBattleInput { get; set; }

	public ClientInstance? Instance;

	public override void _Ready()
	{
		// Set username from cmd line args
		var args = OS.GetCmdlineArgs();
		ConnectingUsername = args.Length > 2 ? args[2] : (args.Length > 0 ? args[^1] : "Elfiawesome");
		GetWindow().Title = ConnectingUsername;
		Core.Logging.Logger.Identity = ConnectingUsername;

		// Bootstrap type registration for base assemblies
		ScanCoreAssemblies();

		// Setup storage & providers
		var provider = new LocalFileProvider();
		var clientDir = System.Environment.CurrentDirectory;
		var projectRoot = provider.GetFullPath(provider.Combine(provider.Combine(clientDir, ".."), ".."));
		var gamedataPath = provider.Combine(projectRoot, "gamedata");
		Storage = new StorageManager(gamedataPath, provider);

		// Start!
		Core.Logging.Logger.Info(ConnectingUsername);
		if (ConnectingUsername == "Elfiawesome")
		{
			StartIntegratedServer();
		}
		else
		{
			JoinServer();
		}
	}

	private void StartIntegratedServer()
	{
		var sessionName = "session_1";
		IntegratedServer = new Server.Server(Storage, sessionName);

		var modDirs = Storage.AllModDirectories;
		ModLoader.ModLoader modLoader = new(modDirs);
		modLoader.Setup();

		SetupModClient(modLoader);
		SetupModServer(modLoader, IntegratedServer);

		var localListener = new LocalListener() { IsSerialized = true };
		var tcpListener = new TcpGameListener(5060);
		IntegratedServer.Start(localListener, tcpListener);
		Connection = localListener.ConnectClient();
	}

	private void JoinServer()
	{
		var modDirs = Storage.AllModDirectories;
		ModLoader.ModLoader modLoader = new(modDirs);
		modLoader.Setup();

		SetupModClient(modLoader);

		var tcpClient = new TcpClient("127.0.0.1", 5060);
		Connection = new TcpConnection(tcpClient);
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

		OnProcess?.Invoke();
	}

	public void SwitchInstance(ClientInstance newInstance)
	{
		if (Instance != null) { RemoveChild(Instance); Instance.onPacketSent -= SendPacket; }
		
		Instance = newInstance;
		Instance.onPacketSent += SendPacket;
		Instance.ClientRegistry = ClientRegistry;
		Instance.BattleEngineRegistry = BattleEngineRegistry;
		AddChild(Instance);
	}

	public void SendPacket(IPacket packet)
		=> Connection?.Send(packet);

	private void HandleIncomingPacket(IPacket packet)
	{
		Core.Logging.Logger.Debug($"Client received packet from server: {packet.GetType().Name}");
		PacketContextClient ctx = new() { Session = this };
		Instance?.OnPacket(packet, ctx);
		ClientRegistry.PacketHandlers.Execute(ctx, packet);
	}

	// TODO REMOVE LATER
	public void SetDebugStatus(string value) => GetNode<Node>("Control/VBoxContainer/Status").Set("content", value);
	public void SetDebugWorld(string value) => GetNode<Node>("Control/VBoxContainer/World").Set("content", value);
	public void SetDebugPlayers(string value) => GetNode<Node>("Control/VBoxContainer/Players").Set("content", value);

	public override void _ExitTree() => ExitCleanup();

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest) { ExitCleanup(); }
	}

	private void ExitCleanup()
	{
		GD.Print("Cleaning up");
		Connection?.Disconnect();
		IntegratedServer?.Stop();
		GetTree().Quit();
	}

	private void ScanCoreAssemblies()
	{
		Core.Data.DataTagTypeRegistry.ScanAssembly(typeof(Core.Data.DataTag).Assembly); // Load core
		Core.Data.DataTagTypeRegistry.ScanAssembly(typeof(ModLoader.ModLoader).Assembly);
		Core.Data.DataTagTypeRegistry.ScanAssembly(typeof(BattleEngine.BattleEngine).Assembly); // Already done in BattleEngine, but just in case
	}

	private void SetupModServer(ModLoader.ModLoader modLoader, Server.Server server)
	{
		var serverContent = modLoader.GetContentServer().ToList();

		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => server.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => server.LoadMod(m, serverContent));
	}

	private void SetupModClient(ModLoader.ModLoader modLoader)
	{
		var clientContent = modLoader.GetContentClient().ToList();

		new VanillaMod().OnLoad(ClientRegistry, clientContent); // TODO: Load dynamically via scanning current assembly bruh
		modLoader.LoadModEntry<IClientMod>().ForEach(m => LoadMod(m, clientContent));
		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => LoadMod(m, clientContent));

		// If loaded client, use its shared battle engine registry to save on memory
		IntegratedServer?.OverrideRegistry(BattleEngineRegistry);
	}

	private void LoadMod(IClientMod mod, List<ModContentResult> modContents)
		=> mod.OnLoad(ClientRegistry, modContents);

	private void LoadMod(IBattleEngineMod mod, List<ModContentResult> modContents)
		=> mod.OnLoad(BattleEngineRegistry, modContents);
}
