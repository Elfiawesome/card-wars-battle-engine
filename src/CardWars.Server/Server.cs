using System.Diagnostics;
using CardWars.BattleEngine;
using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Server.Listener;
using CardWars.Core.Network.Transport;
using CardWars.Server.Packet;
using CardWars.ModLoader;
using CardWars.Server.Session;
using CardWars.Core.Storage;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;

namespace CardWars.Server;

public class Server
{
	private static readonly TimeSpan _tickRate = TimeSpan.FromMilliseconds(16);
	private static readonly TimeSpan _unauthenticatedTimeout = TimeSpan.FromSeconds(30);

	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();
	public IReadOnlyDictionary<Guid, PlayerSession> PlayerSessions => _playerSessions;

	public StorageManager Storage { get; }
	public SessionStorage Session { get; }

	public Action<IConnection>? OnUnauthenticatedConnectionReceived { get; set; }
	public Action<IConnection>? OnUnauthenticatedConnectionRemoved { get; set; }
	public Action<PlayerSession>? OnAddPlayer { get; set; }
	public Action<PlayerSession>? OnRemovePlayer { get; set; }
	public Action<IServerInstance>? OnAddInstance { get; set; }
	public Action<IServerInstance>? OnRemoveInstance { get; set; }
	// public Action<IServerInstance>? OnIntervalSave { get; set; }

	private readonly object _sync = new();
	private readonly List<IListener> _listeners = [];
	private readonly Dictionary<Guid, PlayerSession> _playerSessions = [];
	private readonly Dictionary<IConnection, DateTime> _unauthenticatedConnections = [];
	private readonly Dictionary<Guid, IServerInstance> _instances = [];
	private readonly Dictionary<(ResourceId ProviderId, string Id), Guid> _instanceMapping = [];
	// private double _intervalSaveAccumulator = 0;
	private CancellationTokenSource? _cts;

	public Server(StorageManager storage, string sessionName = "default")
	{
		Storage = storage;
		Session = storage.OpenSession(sessionName);
		ScanCoreAssemblies();
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
		// Save game states
		SaveInstances();
		SavePlayers();

		_cts?.Cancel();
		foreach (var listener in _listeners) listener.Stop();

		// Disconnect
		lock (_sync)
		{
			foreach (var (_, session) in _playerSessions)
			{
				session.CurrentInstance?.RemovePlayer(session);
				session.Connection.Disconnect();
			}
			_playerSessions.Clear();
			// _instances.Clear();
			// _instanceKeys.Clear();

			foreach (var (conn, _) in _unauthenticatedConnections)
				conn.Disconnect();
			_unauthenticatedConnections.Clear();
		}
		Logger.Info("Server stopped.");
	}

	private void OnConnectionReceived(IConnection connection)
	{
		Logger.Debug($"Server: A new unauthenticated connection was received.");
		AddUnauthenticatedConnection(connection);
	}

	// --- Instance Handling ---
	public IServerInstance CreateInstance(ResourceId providerId, string instanceSaveId)
	{
		IServerInstance instance;
		lock (_sync)
		{
			var provider = Registry.ServerInstanceProviders.Get(providerId)
						?? throw new InvalidOperationException($"No server instance provider registered for '{providerId}'.");

			instance = provider.Create(instanceSaveId);
			_instances[instance.InstanceId] = instance;
			_instanceMapping[(providerId, instanceSaveId)] = instance.InstanceId;
		}
		OnAddInstance?.Invoke(instance);
		return instance;
	}

	public void RemoveInstance(Guid instanceId)
	{
		// What if I remove an instance with players in it? TODO:...
		lock (_sync)
		{
			if (_instances.TryGetValue(instanceId, out var instance))
			{
				var provider = Registry.ServerInstanceProviders.Get(instance.InstanceProviderId)
						?? throw new InvalidOperationException($"No server instance provider registered for '{instance.InstanceProviderId}'.");

				provider.Save(instance, Session.InstancesDir);

				_instances.Remove(instanceId);
				foreach (var key in _instanceMapping.Where(kv => kv.Value == instanceId).Select(kv => kv.Key).ToList())
				{
					_instanceMapping.Remove(key);
				}
				OnRemoveInstance?.Invoke(instance);
			}
		}
	}

	public Guid? GetInstanceMapped(ResourceId providerId, string instanceSaveId)
	{
		if (_instanceMapping.TryGetValue((providerId, instanceSaveId), out var instanceId))
		{
			return instanceId;
		}
		return null;
	}

	public void EnterInstance(PlayerSession player, ResourceId providerId, string instanceSaveId)
	{
		var instanceId = GetInstanceMapped(providerId, instanceSaveId)
						?? CreateInstance(providerId, instanceSaveId).InstanceId;
		EnterInstance(player, instanceId);
	}

	public void EnterInstance(PlayerSession player, Guid instanceId)
	{
		LeaveInstance(player);
		lock (_sync)
		{
			if (_instances.TryGetValue(instanceId, out var instance))
			{
				instance.AddPlayer(player);
				player.CurrentInstance = instance;
			}
		}
	}

	public void LeaveInstance(PlayerSession player)
	{
		player.CurrentInstance?.RemovePlayer(player);
		player.CurrentInstance = null;
	}
	public void SaveInstances()
	{
		lock (_sync)
		{
			foreach (var instance in _instances.Values)
			{
				var provider = Registry.ServerInstanceProviders.Get(instance.InstanceProviderId);
				provider?.Save(instance, Session.InstancesDir);
			}
		}
	}

	// --- Player Session Handling ---
	public void AddUnauthenticatedConnection(IConnection connection)
	{
		lock (_sync) { _unauthenticatedConnections[connection] = DateTime.UtcNow; }
		OnUnauthenticatedConnectionReceived?.Invoke(connection);
	}

	public void RemoveUnauthenticatedConnection(IConnection connection)
	{
		lock (_sync) { _unauthenticatedConnections.Remove(connection); }
		OnUnauthenticatedConnectionRemoved?.Invoke(connection);
	}

	public void AddPlayer(PlayerSession player)
	{
		lock (_sync) { _playerSessions.Add(player.PlayerId, player); }
		OnAddPlayer?.Invoke(player);
	}

	public void RemovePlayer(PlayerSession player)
	{
		LeaveInstance(player);
		Session.SavePlayer(player.PlayerId, DataTagMapper.ToTag(player, false));
		lock (_sync) { _playerSessions.Remove(player.PlayerId); }
		OnRemovePlayer?.Invoke(player);
	}

	public void SavePlayers()
	{
		lock (_sync)
		{
			foreach (var p in _playerSessions)
			{
				Logger.Debug(p.Key.ToString());
				Logger.Debug(p.Value.TimePlayed.ToString());
				Session.SavePlayer(p.Key, DataTagMapper.ToTag(p.Value, false));
			}
		}
	}

	// --- Loop ---
	private void ServerLoop(CancellationToken token)
	{
		var stopwatch = Stopwatch.StartNew();

		while (!token.IsCancellationRequested)
		{
			stopwatch.Restart();

			lock (_sync)
			{
				ProcessDisconnections();
				ProcessPackets();
			}

			foreach (var instance in _instances.Values)
			{
				instance.Tick((float)_tickRate.TotalSeconds);
			}
			foreach (var player in _playerSessions)
			{
				player.Value.TimePlayed += 1;
			}

			var remaining = _tickRate - stopwatch.Elapsed;
			if (remaining > TimeSpan.Zero)
				token.WaitHandle.WaitOne(remaining);
		}
	}

	private void ProcessDisconnections()
	{
		var now = DateTime.UtcNow;

		var timedOut = _unauthenticatedConnections.Where(kv => now - kv.Value > _unauthenticatedTimeout).ToList();
		foreach (var (conn, _) in timedOut)
		{
			_unauthenticatedConnections.Remove(conn);
			conn.Disconnect();
			Logger.Debug($"Server: An unauthenticated connection timed out and was closed.");
		}

		var deadUnauthenticated = _unauthenticatedConnections.Where(kv => !kv.Key.IsConnected).ToList();
		foreach (var (conn, _) in deadUnauthenticated)
		{
			_unauthenticatedConnections.Remove(conn);
			Logger.Debug($"Server: An unauthenticated connection disconnected.");
		}

		var disconnected = _playerSessions
			.Where(kv => !kv.Value.Connection.IsConnected)
			.ToList();
		foreach (var (id, session) in disconnected)
		{
			RemovePlayer(session);
			Logger.Debug($"Server: Player [{id}] disconnected.");
		}
	}

	private void ProcessPackets()
	{
		foreach (var (_, playerSession) in _playerSessions)
		{
			var conn = playerSession.Connection;
			while (conn.TryReceive(out var packet))
			{
				if (packet == null) continue;
				HandleLocalPacket(playerSession, packet);
				HandleGlobalPacket(playerSession, packet);
			}
		}

		foreach (var (conn, _) in _unauthenticatedConnections)
		{
			while (conn.TryReceive(out var packet))
			{
				if (packet == null) continue;
				HandleUnauthenticatedPacket(conn, packet);
			}
		}
	}


	// --- Handle Packets ---
	public void HandleUnauthenticatedPacket(IConnection connection, IPacket packet)
		=> Registry.UnauthenticatedPacketHandlers.Execute(
			new PacketUnauthenticatedContextServer() { Server = this, Connection = connection },
			packet);

	public void HandleLocalPacket(PlayerSession playerSession, IPacket packet)
		=> playerSession.CurrentInstance?.HandlePacket(playerSession, packet);

	public void HandleGlobalPacket(PlayerSession playerSession, IPacket packet)
		=> Registry.PacketHandlers.Execute(
			new PacketContextServer() { Server = this, PlayerSession = playerSession },
			packet);


	private void ScanCoreAssemblies()
	{
		DataTagTypeRegistry.ScanAssembly(typeof(DataTag).Assembly); // Load core
		DataTagTypeRegistry.ScanAssembly(typeof(ModLoader.ModLoader).Assembly);
		DataTagTypeRegistry.ScanAssembly(typeof(BattleEngine.BattleEngine).Assembly); // Already done in BattleEngine, but just in case
		DataTagTypeRegistry.ScanAssembly(typeof(Server).Assembly);
	}
}
