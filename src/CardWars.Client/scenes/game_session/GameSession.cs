using CardWars.BattleEngine;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public ClientRegistry ClientRegistry { get; init; } = new();

	// The server instance (only populated if we are hosting)
	public Server.Server? IntegratedServer { get; private set; }

	// The connection used to talk to the server (TCP or Local)
	public IConnection? Connection { get; private set; }

	public override void _Ready()
	{
		ModLoader.ModLoader modLoader = new(@"C:\Users\Elfiyan\Documents\Projects\card-wars-battle-engine\mods");
		modLoader.Setup();
		modLoader.LoadModEntry<IClientMod>().ForEach(m => m.OnLoad(ClientRegistry));

		StartIntegratedServer(modLoader);

		// Example: Sending a test packet immediately upon connecting
		// Connection?.Send(new JoinWorldPacket { PlayerName = "Elfiyan" });
	}

	private void StartIntegratedServer(ModLoader.ModLoader modLoader)
	{
		IntegratedServer = new Server.Server();
		modLoader.LoadModEntry<IBattleEngineMod>().ForEach(m => IntegratedServer.LoadMod(m));
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
		ClientRegistry.PacketHandlers.Execute(new PacketContextClient(), packet);
	}

	public override void _ExitTree()
	{
		Connection?.Disconnect();
		IntegratedServer?.Stop();
	}
}
