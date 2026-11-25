using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CardWars.BattleEngine;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.profile;
using CardWars.Core.Logging;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using CardWars.ModLoader;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class GameSession : Node
{
	public ClientSession? Client { get; private set; }
	public Server.Server? IntegratedServer { get; private set; }
	public StorageManager? Storage { get; private set; }

	public string ConnectingUsername = "";
	public string TargetIpAddress = "127.0.0.1";
	public int TargetPort = 5060;

	public override void _Ready()
	{
		// Set username from cmd line args
		var args = OS.GetCmdlineArgs();
		ConnectingUsername = args.Length > 2 ? args[2] : "Elfiawesome";
		TargetIpAddress = args.Length > 3 ? args[3] : TargetIpAddress;
		TargetPort = args.Length > 4 ? args[4].ToInt() : TargetPort;
		GetWindow().Title = ConnectingUsername;
		Log.Identity = ConnectingUsername;

		// Setup storage & providers
		var provider = new LocalFileProvider();
		var clientDir = System.Environment.CurrentDirectory;
		var projectRoot = provider.GetFullPath(provider.Combine(provider.Combine(clientDir, ".."), ".."));
		var gamedataPath = provider.Combine(projectRoot, "gamedata");
		Storage = new StorageManager(gamedataPath, provider);
		var profileRoot = new StoragePath(provider.Combine(gamedataPath, ".userdata"), provider); // We use this temp folder first but we can use ProjectSettings.GlobalizePath("user://profiles") later
		var profileStore = new ClientProfileStore(profileRoot);

		// Start!
		Log.Info($"Game Session started with username: {ConnectingUsername}");
		StartSession(profileStore);
	}

	private void StartSession(ClientProfileStore profileStore)
	{
		if (Storage is null) { return; }

		// Set up ModLoader & scan for all DLLs Mod in dir
		var modLoader = new ModLoader.ModLoader(Storage.AllModDirectories);
		modLoader.Setup();

		// Get all contents from every mod
		var content = GameContent.Load(modLoader);

		// Create a shared Battle registry
		var battleRegistry = new BattleEngineRegistry();
		foreach (var mod in modLoader.LoadModEntry<IBattleEngineMod>())
			mod.OnLoad(battleRegistry, content.BattleData);

		var clientRegistry = new ClientRegistry();
		clientRegistry.RegisterExtension(new BattleClientRuntime(battleRegistry));

		Client = new ClientSession(this, ConnectingUsername, clientRegistry, profileStore);
		Client.OnDebugStatus = SetDebugStatus;
		Client.OnDebugWorld = SetDebugWorld;
		Client.OnDebugPlayers = SetDebugPlayers;

		// Load client mod (built within the current assembly)
		var clientMods = CreateModEntries<IClientMod>().Concat(modLoader.LoadModEntry<IClientMod>()); // Get all  IClientMod in the current assembly + modloader's loaded mod assemblies
		Client.LoadContent(clientMods, content.ClientPacks, content.ClientData, content.BattleAssets);

		// Start either server or client!
		if (ConnectingUsername == "Elfiawesome") { StartIntegratedServer(modLoader, content, battleRegistry); }
		else { JoinServer(); }
	}

	private void StartIntegratedServer(ModLoader.ModLoader modLoader, GameContent content, BattleEngineRegistry battleRegistry)
	{
		if (Storage is null || Client is null) { return; }

		IntegratedServer = new Server.Server(Storage, "session_1");
		IntegratedServer.Registry.RegisterExtension(battleRegistry);
		foreach (var mod in modLoader.LoadModEntry<IServerMod>())
			mod.OnLoad(IntegratedServer, content.ServerData);

		var localListener = new LocalListener() { IsSerialized = true };
		var tcpListener = new TcpGameListener(TargetPort);
		IntegratedServer.Start(localListener, tcpListener);
		Client.Connect(localListener.ConnectClient());
	}

	private void JoinServer()
	{
		if (Client is null) { return; }

		var tcpClient = new TcpClient(TargetIpAddress, TargetPort);
		Client.Connect(new TcpConnection(tcpClient));
	}

	public override void _Process(double delta) => Client?.Process();

	private void SetDebugStatus(string value) => GetNode<Node>("Control/VBoxContainer/Status").Set("content", value);
	private void SetDebugWorld(string value) => GetNode<Node>("Control/VBoxContainer/World").Set("content", value);
	private void SetDebugPlayers(string value) => GetNode<Node>("Control/VBoxContainer/Players").Set("content", value);

	public override void _ExitTree() => ExitCleanup();

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest) { ExitCleanup(); }
	}

	private void ExitCleanup()
	{
		GD.Print("Cleaning up");
		Client?.Disconnect();
		IntegratedServer?.Stop();
		GetTree().Quit();
	}

	private static IEnumerable<TMod> CreateModEntries<TMod>()
		where TMod : class, IModEntry
		=> typeof(GameSession).Assembly
			.GetTypes()
			.Where(type => typeof(TMod).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
			.Select(type => Activator.CreateInstance(type))
			.OfType<TMod>();
}
