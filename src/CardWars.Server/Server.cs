using CardWars.BattleEngine;
using CardWars.Core.Logging;
using CardWars.Server.Listener;
using CardWars.Core.Network.Transport;
using CardWars.Server.Packet;
using CardWars.ModLoader;

namespace CardWars.Server;

public class Server
{
	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();

	private readonly List<IListener> _listeners = [];
	private readonly List<IConnection> _connections = [];
	private CancellationTokenSource? _cts;

	public Server() { }

	public void LoadMod(IServerMod mod) => mod.OnLoad(Registry);
	public void LoadMod(IBattleEngineMod mod, List<ModContentResult> modContents) => mod.OnLoad(SharedBattleEngineRegistry, modContents);

	public void Start(params IListener[] listeners)
	{
		foreach (var listener in listeners)
		{
			listener.OnNewConnection = OnConnectionReceived;
			listener.Start();
			_listeners.Add(listener);
		}

		_cts = new CancellationTokenSource();
		Task.Run(() => ServerLoop(_cts.Token));
		Logger.Info("Server started successfully.");
	}

	public void Stop()
	{
		_cts?.Cancel();
		foreach (var listener in _listeners) listener.Stop();

		lock (_connections)
		{
			foreach (var conn in _connections) conn.Disconnect();
			_connections.Clear();
		}
		Logger.Info("Server stopped.");
	}

	private void OnConnectionReceived(IConnection connection)
	{
		lock (_connections)
		{
			_connections.Add(connection);
		}
		Logger.Info("Server: A new client connected.");
	}

	private void ServerLoop(CancellationToken token)
	{
		// 60 Ticks per second
		var tickRate = TimeSpan.FromMilliseconds(16);

		while (!token.IsCancellationRequested)
		{
			lock (_connections)
			{
				for (int i = _connections.Count - 1; i >= 0; i--)
				{
					var conn = _connections[i];
					if (!conn.IsConnected)
					{
						_connections.RemoveAt(i);
						Logger.Info("Server: Client disconnected.");
						continue;
					}

					// Process all packets sent by this client
					while (conn.TryReceive(out var packet))
					{
						if (packet != null)
						{
							var context = new PacketContextServer();
							Registry.PacketHandlers.Execute(context, packet);
						}
					}
				}
			}
			Logger.Info("Server: Tick!");
			Thread.Sleep(tickRate);
		}
	}
}