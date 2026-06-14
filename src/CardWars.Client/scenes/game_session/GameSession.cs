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
	public Label? DebugLabel;

	public override void _Ready()
	{
		DebugLabel = GetNode<Label>("DebugLabel");
		
		// Set username from cmd line args
		ConnectingUsername = OS.GetCmdlineArgs()[2];
		GetWindow().Title = ConnectingUsername;
		Core.Logging.Logger.Identity = ConnectingUsername;

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
		modLoader.LoadModEntry<IServerMod>().ForEach(m => m.OnServerStart(IntegratedServer));

		var localListener = new LocalListener();
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

	public override void _ExitTree()
	{
		Connection?.Disconnect();
		IntegratedServer?.Stop();
	}
}
