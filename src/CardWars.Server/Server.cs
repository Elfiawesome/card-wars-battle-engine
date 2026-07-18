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

namespace CardWars.Server;

public class Server
{
	private static readonly TimeSpan _tickRate = TimeSpan.FromMilliseconds(16);
	private static readonly TimeSpan _unauthenticatedTimeout = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan _autoSaveInterval = TimeSpan.FromSeconds(30);

	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();
	public IReadOnlyDictionary<Guid, PlayerSession> PlayerSessions => _playerSessions;
	public IReadOnlyDictionary<Guid, IServerInstance> Instances => _instances;

	public StorageManager Storage { get; }
	public SessionStorage Session { get; }

	public Action<IConnection>? OnUnauthenticatedConnectionReceived { get; set; }
	public Action<IConnection>? OnUnauthenticatedConnectionRemoved { get; set; }
	public Action<PlayerSession>? OnAddPlayer { get; set; }
	public Action<PlayerSession>? OnRemovePlayer { get; set; }
	public Action<IServerInstance>? OnAddInstance { get; set; }
	public Action<IServerInstance>? OnRemoveInstance { get; set; }

	private readonly object _sync = new();
	private readonly List<IListener> _listeners = [];
	private readonly Dictionary<Guid, PlayerSession> _playerSessions = [];
	private readonly Dictionary<IConnection, DateTime> _unauthenticatedConnections = [];
	private readonly Dictionary<Guid, IServerInstance> _instances = [];
	private readonly Dictionary<Guid, InstanceMeta> _instanceMeta = [];
	private readonly Dictionary<(string kind, string key), IServerInstance> _instancesByKey = [];
	private readonly Dictionary<Guid, int> _instancePlayerCounts = [];
	private DateTime _lastAutoSave = DateTime.UtcNow;
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

		lock (_sync)
		{
			foreach (var (_, session) in _playerSessions)
			{
				RemovePlayerFromInstanceInternal(session);
				session.Connection.Disconnect();
			}
			_playerSessions.Clear();

			foreach (var (conn, _) in _unauthenticatedConnections)
				conn.Disconnect();
			_unauthenticatedConnections.Clear();
		}

		SaveAllInstances();
		Logger.Info("Server stopped.");
	}

	private void OnConnectionReceived(IConnection connection)
	{
		Logger.Debug($"Server: A new unauthenticated connection was received.");
		AddUnauthenticatedConnection(connection);
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
		lock (_sync) { _playerSessions.Remove(player.PlayerId); }
		OnRemovePlayer?.Invoke(player);
	}

	// --- Instance Handling ---
	public void AddInstance(IServerInstance instance)
	{
		lock (_sync) { _instances.Add(instance.InstanceId, instance); }
		OnAddInstance?.Invoke(instance);
	}

	public void RemoveInstance(IServerInstance instance)
	{
		lock (_sync) { _instances.Remove(instance.InstanceId); }
		OnRemoveInstance?.Invoke(instance);
	}

	// --- Instance Lifecycle ---
	public IServerInstance GetOrCreateInstance(string kind, string key)
	{
		var descriptor = Registry.InstanceKinds.Get(kind)
			?? throw new InvalidOperationException($"Unknown instance kind: '{kind}'");
		if (descriptor.Policy != InstancePolicy.SingletonByKey)
			throw new InvalidOperationException(
				$"Instance kind '{kind}' is not SingletonByKey; use CreateInstance instead.");

		lock (_sync)
		{
			if (_instancesByKey.TryGetValue((kind, key), out var existing))
				return existing;

			var instance = LoadOrCreate(kind, key, descriptor);
			RegisterInstanceInternal(instance, kind, key, descriptor);
			return instance;
		}
	}

	public IServerInstance? GetInstance(string kind, string key)
	{
		var descriptor = Registry.InstanceKinds.Get(kind)
			?? throw new InvalidOperationException($"Unknown instance kind: '{kind}'");

		lock (_sync)
		{
			if (_instancesByKey.TryGetValue((kind, key), out var existing))
				return existing;

			if (descriptor.Persistence != InstancePersistence.Persistent)
				return null;

			var saved = Session.LoadInstance(kind, key);
			if (saved is not CompoundTag ct) return null;

			var instance = descriptor.Provider.Load(new InstanceLoadContext { Kind = kind, Key = key, SavedData = ct });
			instance.OnLoad(ct);
			RegisterInstanceInternal(instance, kind, key, descriptor);
			Logger.Debug($"Server: Resumed instance kind='{kind}' key='{key}' from disk.");
			return instance;
		}
	}

	public IServerInstance CreateInstance(string kind, string? key = null)
	{
		var descriptor = Registry.InstanceKinds.Get(kind)
			?? throw new InvalidOperationException($"Unknown instance kind: '{kind}'");
		if (descriptor.Policy != InstancePolicy.Multi)
			throw new InvalidOperationException(
				$"Instance kind '{kind}' is not Multi; use GetOrCreateInstance instead.");

		key ??= Guid.NewGuid().ToString();

		lock (_sync)
		{
			if (_instancesByKey.ContainsKey((kind, key)))
				throw new InvalidOperationException(
					$"Instance kind='{kind}' key='{key}' already exists; use GetInstance to resume it.");

			var instance = descriptor.Provider.Create(new InstanceCreateContext { Kind = kind, Key = key });
			instance.OnCreate();
			RegisterInstanceInternal(instance, kind, key, descriptor);
			if (descriptor.Persistence == InstancePersistence.Persistent)
				SaveInstanceInternal(instance, key);
			Logger.Debug($"Server: Created new instance kind='{kind}' key='{key}'.");
			return instance;
		}
	}

	public void DestroyInstance(IServerInstance instance)
	{
		string kind;
		string key;
		lock (_sync)
		{
			if (!_instanceMeta.TryGetValue(instance.InstanceId, out var meta))
				return;
			kind = meta.Kind;
			key = meta.Key;

			_instances.Remove(instance.InstanceId);
			_instanceMeta.Remove(instance.InstanceId);
			_instancesByKey.Remove((kind, key));
			_instancePlayerCounts.Remove(instance.InstanceId);
		}

		instance.OnDestroy();
		OnRemoveInstance?.Invoke(instance);

		if (Registry.InstanceKinds.Get(kind)?.Persistence == InstancePersistence.Persistent)
			Session.DeleteInstance(kind, key);
		Logger.Debug($"Server: Destroyed instance kind='{kind}' key='{key}'.");
	}

	public void SaveAllInstances()
	{
		List<(IServerInstance instance, string key, string kind)> snapshot;
		lock (_sync)
		{
			snapshot = _instanceMeta
				.Where(kv => Registry.InstanceKinds.Get(kv.Value.Kind)?.Persistence == InstancePersistence.Persistent)
				.Select(kv => (_instances[kv.Key], kv.Value.Key, kv.Value.Kind))
				.ToList();
		}

		foreach (var (instance, key, kind) in snapshot)
		{
			try
			{
				var tag = instance.OnSave();
				Session.SaveInstance(kind, key, tag);
			}
			catch (Exception ex)
			{
				Logger.Error($"Server: Failed to save instance kind='{kind}' key='{key}': {ex.Message}");
			}
		}
	}

	// --- Player <-> Instance Assignment ---
	public void AssignPlayerToInstance(PlayerSession player, IServerInstance instance)
	{
		lock (_sync)
		{
			if (player.CurrentInstance == instance) return;
			if (player.CurrentInstance != null)
				RemovePlayerFromInstanceInternal(player);

			player.CurrentInstance = instance;
			instance.AddPlayer(player);
			_instancePlayerCounts[instance.InstanceId] =
				_instancePlayerCounts.GetValueOrDefault(instance.InstanceId) + 1;
		}
	}

	public void RemovePlayerFromInstance(PlayerSession player)
	{
		lock (_sync) { RemovePlayerFromInstanceInternal(player); }
	}

	private void RemovePlayerFromInstanceInternal(PlayerSession player)
	{
		var instance = player.CurrentInstance;
		if (instance == null) return;

		player.CurrentInstance = null;
		instance.RemovePlayer(player);

		if (!_instanceMeta.TryGetValue(instance.InstanceId, out var meta)) return;
		var count = _instancePlayerCounts.GetValueOrDefault(instance.InstanceId) - 1;
		if (count > 0)
		{
			_instancePlayerCounts[instance.InstanceId] = count;
			return;
		}

		_instancePlayerCounts[instance.InstanceId] = 0;

		// Singleton persistent worlds unload + save when empty. Multi (battles) stay until DestroyInstance.
		if (meta.Descriptor.Policy != InstancePolicy.SingletonByKey
			|| meta.Descriptor.Persistence != InstancePersistence.Persistent)
			return;

		try
		{
			var tag = instance.OnSave();
			Session.SaveInstance(meta.Kind, meta.Key, tag);
		}
		catch (Exception ex)
		{
			Logger.Error($"Server: Failed to save instance kind='{meta.Kind}' key='{meta.Key}' on unload: {ex.Message}");
		}

		instance.OnDestroy();
		_instances.Remove(instance.InstanceId);
		_instanceMeta.Remove(instance.InstanceId);
		_instancesByKey.Remove((meta.Kind, meta.Key));
		_instancePlayerCounts.Remove(instance.InstanceId);
		OnRemoveInstance?.Invoke(instance);
		Logger.Debug($"Server: Unloaded empty instance kind='{meta.Kind}' key='{meta.Key}'.");
	}

	private void RegisterInstanceInternal(IServerInstance instance, string kind, string key, InstanceKindDescriptor descriptor)
	{
		_instances.Add(instance.InstanceId, instance);
		_instanceMeta[instance.InstanceId] = new InstanceMeta(kind, key, descriptor);
		_instancesByKey[(kind, key)] = instance;
		_instancePlayerCounts[instance.InstanceId] = 0;
		OnAddInstance?.Invoke(instance);
	}

	private void SaveInstanceInternal(IServerInstance instance, string key)
	{
		var meta = _instanceMeta[instance.InstanceId];
		var tag = instance.OnSave();
		Session.SaveInstance(meta.Kind, key, tag);
	}

	private IServerInstance LoadOrCreate(string kind, string key, InstanceKindDescriptor descriptor)
	{
		var saved = Session.LoadInstance(kind, key);
		if (saved is CompoundTag ct)
		{
			var instance = descriptor.Provider.Load(new InstanceLoadContext { Kind = kind, Key = key, SavedData = ct });
			instance.OnLoad(ct);
			Logger.Debug($"Server: Loaded instance kind='{kind}' key='{key}' from disk.");
			return instance;
		}

		var fresh = descriptor.Provider.Create(new InstanceCreateContext { Kind = kind, Key = key });
		fresh.OnCreate();
		if (descriptor.Persistence == InstancePersistence.Persistent)
		{
			var tag = fresh.OnSave();
			Session.SaveInstance(kind, key, tag);
		}
		Logger.Debug($"Server: Created new instance kind='{kind}' key='{key}'.");
		return fresh;
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

			if (DateTime.UtcNow - _lastAutoSave > _autoSaveInterval)
			{
				_lastAutoSave = DateTime.UtcNow;
				SaveAllInstances();
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
			RemovePlayerFromInstanceInternal(session);
			_playerSessions.Remove(id);
			OnRemovePlayer?.Invoke(session);
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
}

internal sealed record InstanceMeta(string Kind, string Key, InstanceKindDescriptor Descriptor);
