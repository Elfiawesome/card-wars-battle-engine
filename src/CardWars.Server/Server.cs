using CardWars.BattleEngine;
using CardWars.Core.Logging;
using CardWars.Server.Listener;
using CardWars.Core.Network.Transport;
using CardWars.Server.Packet;
using CardWars.ModLoader;
using CardWars.Server.Session;

namespace CardWars.Server;

public class Server
{
	public ServerRegistry Registry { get; } = new();
	public BattleEngineRegistry SharedBattleEngineRegistry { get; } = new();

	private readonly List<IListener> _listeners = [];
	private readonly Dictionary<Guid, PlayerSession> _playerSessions = [];
	private readonly Dictionary<Guid, IServerInstance> _instances = [];
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
		lock (_playerSessions)
		{
			_playerSessions.Add(playerId, new PlayerSession(playerId, connection));
		}
		Logger.Info($"Server: A new client [{playerId}] connected.");
	}

	public void CreateInstance(IServerInstance instance)
		=> _instances.Add(instance.InstanceId, instance);

	public void RemoveInstance(IServerInstance instance)
		=> _instances.Remove(instance.InstanceId);

	public void AddPlayerToInstance(PlayerSession playerId, IServerInstance instanceId)
		=> instanceId.AddPlayer(playerId);

	private void ServerLoop(CancellationToken token)
	{
		// 60 Ticks per second
		var tickRate = TimeSpan.FromMilliseconds(16);

		while (!token.IsCancellationRequested)
		{
			lock (_playerSessions)
			{
				List<Guid> disconnected = [.. _playerSessions
					.Where(kv => !kv.Value.Connection.IsConnected)
					.Select(kv => kv.Key)];

				foreach (var id in disconnected)
				{
					_playerSessions.Remove(id);
					Logger.Info($"Server: Client [{id}] disconnected.");
				}

				foreach (var (playerId, playerSession) in _playerSessions)
				{
					var conn = playerSession.Connection;
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