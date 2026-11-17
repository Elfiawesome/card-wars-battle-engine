using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CardWars.BattleEngine;
using CardWars.BattleEngine.Input;
// Phase 4 (R5/R8): the `using CardWars.Client.scripts.cardwars;` import was removed along with the
// privileged `new VanillaMod()` path below. Core no longer references the vanilla namespace; the
// vanilla client entry is discovered by scanning via modLoader.LoadModEntry<IClientMod>() like any
// third-party mod. PROVISIONAL / UNCOMPILED: Godot 4.7 is absent here; validate in Phase 6.
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
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
	// Session-owned logger (Phase 3, R9/R1). Replaces the former static Log singleton: Identity
	// and the OnLog sink live on this instance, and it is threaded to the Server, ModLoader,
	// StorageManager, the registries and transport the same way Data is. NOTE: CardWars.Client
	// targets Godot 4.7 which is not installed in the engine CI sandbox, so this file is NOT
	// compiled/verified there; the edits are kept minimal, mechanical and internally consistent
	// with the engine changes (validate in Phase 6). Declared before the registries so its value
	// is available to their initializers (field initializers run in textual order).
	public Logger Logger { get; init; } = new();

	// Phase 4/R9: thread this session's owned Logger into the registries so client mod-load
	// diagnostics (the vanilla mod's "Registered ..." lines) reach the session sink instead of
	// falling back to NullLogger.Instance. Mirrors the engine's BattleEngineRegistry(ILogger?).
	public ClientRegistry ClientRegistry { get; init; }
	public BattleEngineRegistry BattleEngineRegistry { get; init; }

	// Session-owned data context, consistent with Server.Data. Defaults to DataContext.Default
	// so Phase 1 behavior is preserved while other client call sites still use the facade.
	public DataContext Data { get; init; } = DataContext.Default;

	public GameSession()
	{
		ClientRegistry = new ClientRegistry(Logger);
		BattleEngineRegistry = new BattleEngineRegistry(Logger);
	}

	public Server.Server? IntegratedServer { get; private set; }
	public IConnection? Connection { get; private set; }
	public StorageManager? Storage { get; private set; } = null;

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
		Logger.Identity = ConnectingUsername;

		// Route this session's owned logger to the Godot console sink (replaces the old static
		// Log.OnLog subscription that lived in the LoggerHook autoload).
		GetNodeOrNull<scripts.core.global.LoggerHook>("/root/LoggerHook")?.Attach(Logger);

		// Bootstrap type registration for base assemblies
		ScanCoreAssemblies();

		// Setup storage & providers
		var provider = new LocalFileProvider();
		var clientDir = System.Environment.CurrentDirectory;
		var projectRoot = provider.GetFullPath(provider.Combine(provider.Combine(clientDir, ".."), ".."));
		var gamedataPath = provider.Combine(projectRoot, "gamedata");
		// Pass this session's owned DataContext so storage (de)serialization is scoped to it
		// (Phase 5). PROVISIONAL / UNCOMPILED: the Godot client is not built in this environment;
		// validate on a Godot VM in Phase 6.
		Storage = new StorageManager(gamedataPath, provider, Logger, Data);

		// Start!
		Logger.Info($"Game Session started with username: {ConnectingUsername}");
		
		if (ConnectingUsername == "Elfiawesome") { StartIntegratedServer(); }
		else { JoinServer(); }
	}

	private void StartIntegratedServer()
	{
		if (Storage is null) { return; }
		var sessionName = "session_1";
		IntegratedServer = new Server.Server(Storage, sessionName, Logger);

		var modDirs = Storage.AllModDirectories;
		ModLoader.ModLoader modLoader = new(modDirs, Data, Logger);
		modLoader.Setup();

		SetupModClient(modLoader);
		SetupModServer(modLoader, IntegratedServer);

		var localListener = new LocalListener() { IsSerialized = true };
		var tcpListener = new TcpGameListener(5060, Logger);
		IntegratedServer.Start(localListener, tcpListener);
		Connection = localListener.ConnectClient();
	}

	private void JoinServer()
	{
		if (Storage is null) { return; }
		var modDirs = Storage.AllModDirectories;
		ModLoader.ModLoader modLoader = new(modDirs, Data, Logger);
		modLoader.Setup();

		SetupModClient(modLoader);

		var tcpClient = new TcpClient("127.0.0.1", 5060);
		Connection = new TcpConnection(tcpClient, Logger);
	}

	public override void _Process(double delta)
	{
		if (Connection is { IsConnected: true } connection)
		{
			while (connection.TryReceive(out var packet))
			{
				if (packet is { } received) HandleIncomingPacket(received);
			}
		}

		OnProcess?.Invoke();
	}

	public void SwitchInstance(ClientInstance newInstance, Guid? playerId)
	{
		if (Instance is { } oldInstance) { RemoveChild(oldInstance); oldInstance.onPacketSent -= SendPacket; }

		Instance = newInstance;
		Instance.onPacketSent += SendPacket;
		Instance.Registry.Client = ClientRegistry;
		Instance.Registry.Battle = BattleEngineRegistry;
		Instance.MyPlayerId = playerId ?? Guid.Empty;
		AddChild(Instance);
	}

	public void SendPacket(IPacket packet)
		=> Connection?.Send(packet);

	private void HandleIncomingPacket(IPacket packet)
	{
		Logger.Debug($"Client received packet from server: {packet.GetType().Name}");
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
		Data.ScanAssembly(typeof(DataTag).Assembly); // Load core
		Data.ScanAssembly(typeof(ModLoader.ModLoader).Assembly);
		Data.ScanAssembly(typeof(BattleEngine.BattleEngine).Assembly); // Already done in BattleEngine, but just in case
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
		var battleContent = IntegratedServer is null
			? modLoader.GetContentServer().ToList()
			: clientContent;

		// Phase 4 (R5): client mod load is driven SOLELY by scanning discovery. The privileged
		// `new VanillaMod().OnLoad(...)` path was removed; vanilla's client entry is picked up here
		// through LoadModEntry<IClientMod>() like any third-party mod (its assembly is among the
		// loaded mod assemblies). Core no longer constructs or references the vanilla namespace (R8).
		modLoader.LoadModEntry<IClientMod>().ForEach(m => LoadMod(m, clientContent));
		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => LoadMod(m, battleContent)); // TODO: Maybe content is loaded and passed on by server. So we create a new battle registry each session and is given by server??

		// If loaded client, use its shared battle engine registry to save on memory
		IntegratedServer?.OverrideRegistry(BattleEngineRegistry);
	}

	private void LoadMod(IClientMod mod, List<ModContentResult> modContents)
		=> mod.OnLoad(ClientRegistry, modContents);

	// Phase 4 (FEAT-001): IBattleEngineMod.OnLoad now takes the owning DataContext so content tags
	// baked at mod-load time serialize through this session's context rather than the static
	// DataTagMapper facade (Phase 2). Pass this session's Data, consistent with
	// BattleEngine.LoadMod / Server.LoadMod on the headless side.
	private void LoadMod(IBattleEngineMod mod, List<ModContentResult> modContents)
		=> mod.OnLoad(BattleEngineRegistry, Data, modContents);
}
