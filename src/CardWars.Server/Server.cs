using CardWars.BattleEngine;
using CardWars.Core.Logging;
using CardWars.Server.Listener;
using CardWars.Core.Network.Transport;
using CardWars.Server.Packet;
using CardWars.ModLoader;
using CardWars.Server.Session;
using CardWars.Core.Storage;
using CardWars.Core.Network.Packet;

namespace CardWars.Server;

public class Server
{
	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();
	public IReadOnlyDictionary<Guid, PlayerSession> PlayerSessions => _playerSessions;

	public StorageManager Storage { get; }
	public SessionStorage Session { get; }

	public Action<IConnection>? OnPendingConnectionRequest { get; set; }
	public Action<PlayerSession>? OnAddPlayer { get; set; }
	public Action<PlayerSession>? OnRemovePlayer { get; set; }

	// public Action<PlayerSession>? OnPlayerDisconnected { get; set; }

	private readonly List<IListener> _listeners = [];
	private readonly Dictionary<Guid, PlayerSession> _playerSessions = [];
	private readonly List<IConnection> _pendingConnections = [];
	private readonly Dictionary<Guid, IServerInstance> _instances = [];
	private CancellationTokenSource? _cts;

	public Server(StorageManager storage, string sessionName = "default")
	{
		Storage = storage;
		Session = storage.OpenSession(sessionName);
	}

	public void LoadMod(IServerMod mod, List<ModContentResult> modContents) => mod.OnLoad(this, modContents);
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

		lock (_playerSessions)
		{
			foreach (var playerSession in _playerSessions.ToList())
			{
			}
			_playerSessions.Clear();
		}
		Logger.Info("Server stopped.");
	}

	private void OnConnectionReceived(IConnection connection)
	{
		Logger.Debug($"Server: A new pending connection request was received.");
		AddPendingConnection(connection);
	}

	public void AddPendingConnection(IConnection connection) { _pendingConnections.Add(connection); OnPendingConnectionRequest?.Invoke(connection); }

	public void RemovePendingConnection(IConnection connection) { _pendingConnections.Remove(connection); }

	public void AddPlayer(PlayerSession player) { _playerSessions.Add(player.PlayerId, player); OnAddPlayer?.Invoke(player); }

	public void RemovePlayer(PlayerSession player) { _playerSessions.Remove(player.PlayerId); OnRemovePlayer?.Invoke(player); }

	private void ServerLoop(CancellationToken token)
	{
		var tickRate = TimeSpan.FromMilliseconds(16);

		while (!token.IsCancellationRequested)
		{
			lock (_playerSessions)
			{
				var disconnected = _playerSessions
					.Where(kv => !kv.Value.Connection.IsConnected)
					.ToList();

				// Handle disconnect
				foreach (var (id, session) in disconnected)
				{
					// session.CurrentInstance?.RemovePlayer(session);
					// Remove player
					Logger.Debug($"Server: A connection [{id}] disconnected.");
				}

				// Handle connection packets
				foreach (var (playerId, playerSession) in _playerSessions)
				{
					var conn = playerSession.Connection;
					while (conn.TryReceive(out var packet))
					{
						if (packet == null) continue;
						HandleLocalPacket(playerSession, packet);
						HandleGlobalPacket(playerSession, packet);
					}
				}

				foreach (var pc in _pendingConnections)
				{
					while (pc.TryReceive(out var packet))
					{
						if (packet == null) continue;
						HandlePendingPacket(pc, packet);
					}
				}
			}

			foreach (var instance in _instances.Values)
			{
				instance.Tick((float)tickRate.TotalSeconds);
			}

			// Logger.Info("Server: Tick!");
			Thread.Sleep(tickRate);
		}
	}

	public void HandlePendingPacket(IConnection connection, IPacket packet)
		=> Registry.PendingPacketHandlers.Execute(
			new PacketPendingContextServer() { Server = this, Connection = connection },
			packet);

	public void HandleLocalPacket(PlayerSession playerSession, IPacket packet)
		=> playerSession.CurrentInstance?.HandlePacket(playerSession, packet);

	public void HandleGlobalPacket(PlayerSession playerSession, IPacket packet)
		=> Registry.PacketHandlers.Execute(
			new PacketContextServer() { Server = this, PlayerSession = playerSession },
			packet);
}
