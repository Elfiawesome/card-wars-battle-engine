using System.Linq;
using System.Net.Sockets;
using CardWars.BattleEngine;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();

	public Server.Server? IntegratedServer { get; private set; }
	public IConnection? Connection { get; private set; }
	public StorageManager Storage { get; private set; } = null!;

	public string ConnectingUsername = "";

	public override void _Ready()
	{


		// Set username from cmd line args
		ConnectingUsername = OS.GetCmdlineArgs()[2];
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

		var clientContent = modLoader.GetContentClient().ToList();
		var serverContent = modLoader.GetContentServer().ToList();

		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => IntegratedServer.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IServerMod>().ForEach(m => IntegratedServer.LoadMod(m, serverContent));
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry, clientContent));

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

		var clientContent = modLoader.GetContentClient().ToList();
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry, clientContent));

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
	}

	private void HandleIncomingPacket(IPacket packet)
	{
		Core.Logging.Logger.Debug($"Client received packet from server: {packet.GetType().Name}");
		ClientRegistry.PacketHandlers.Execute(new PacketContextClient() { Session = this }, packet);
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
}
