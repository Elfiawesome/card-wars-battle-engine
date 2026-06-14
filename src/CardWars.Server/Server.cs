using CardWars.BattleEngine;
using CardWars.Core.Logging;
using CardWars.Server.Listener;
using CardWars.Core.Network.Transport;
using CardWars.Server.Packet;
using CardWars.ModLoader;
using CardWars.Server.Session;
using CardWars.Core.Storage;

namespace CardWars.Server;

public class Server
{
	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();
	public IReadOnlyDictionary<Guid, PlayerSession> PlayerSessions => _playerSessions;

	public StorageManager Storage { get; }
	public SessionStorage Session { get; }

	public Action<PlayerSession>? OnPlayerConnected { get; set; }
	public Action<PlayerSession, IServerInstance>? OnPlayerInstanceChanged { get; set; }
	public Action<PlayerSession>? OnPlayerDisconnected { get; set; }

	private readonly List<IListener> _listeners = [];
	private readonly Dictionary<Guid, PlayerSession> _playerSessions = [];
	private readonly Dictionary<Guid, IServerInstance> _instances = [];
	private CancellationTokenSource? _cts;

	public Server(StorageManager storage, string sessionName = "default")
	{
		Storage = storage;
		Session = storage.OpenSession(sessionName);
	}

	public void LoadMod(IServerMod mod, List<ModContentResult> modContents) => mod.OnLoad(Registry, modContents);
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
			foreach (var playerSession in _playerSessions)
			{
				playerSession.Value.Connection.Disconnect();
			}
			_playerSessions.Clear();
		}
		Logger.Info("Server stopped.");
	}

	private void OnConnectionReceived(IConnection connection)
	{
		var playerId = Guid.NewGuid();
		var session = new PlayerSession(playerId, connection);
		lock (_playerSessions)
		{
			_playerSessions.Add(playerId, session);
		}

		Logger.Debug($"Server: A connection [{playerId}] request was received.");
		OnPlayerConnected?.Invoke(session);
	}

	public void SwapPlayerSessionIds(Guid oldPlayerId, Guid newPlayerId)
	{
		lock (_playerSessions)
		{
			if (_playerSessions.TryGetValue(oldPlayerId, out var ps))
			{
				_playerSessions[newPlayerId] = ps;
				ps.PlayerId = newPlayerId;
				_playerSessions.Remove(oldPlayerId);
			}
		}
	}

	public void CreateInstance(IServerInstance instance)
		=> _instances.Add(instance.InstanceId, instance);

	public void RemoveInstance(IServerInstance instance)
		=> _instances.Remove(instance.InstanceId);

	public void AddPlayerToInstance(PlayerSession player, IServerInstance instance)
	{
		player.CurrentInstance = instance;
		instance.AddPlayer(player);
		OnPlayerInstanceChanged?.Invoke(player, instance);
	}

	public void TransferPlayer(PlayerSession player, IServerInstance target)
	{
		lock (_playerSessions)
		{
			player.CurrentInstance?.RemovePlayer(player);
			player.CurrentInstance = target;
			target.AddPlayer(player);
		}
		OnPlayerInstanceChanged?.Invoke(player, target);
	}

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

				foreach (var (id, session) in disconnected)
				{
					session.CurrentInstance?.RemovePlayer(session);
					Session.SavePlayer(id, session.PersistentData);
					_playerSessions.Remove(id);
					OnPlayerDisconnected?.Invoke(session);
					Logger.Debug($"Server: A connection [{id}] disconnected.");
				}

				foreach (var (playerId, playerSession) in _playerSessions)
				{
					var conn = playerSession.Connection;
					while (conn.TryReceive(out var packet))
					{
						if (packet == null) continue;

						if (playerSession.PlayState == PlayState.Play && playerSession.CurrentInstance != null)
						{
							playerSession.CurrentInstance.HandlePacket(playerSession, packet);
						}
						else
						{
							var context = new PacketContextServer() { Server = this, PlayerSession = playerSession };
							Registry.PacketHandlers.Execute(context, packet);
						}
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
}
