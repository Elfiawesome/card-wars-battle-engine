using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CardWars.BattleEngine;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.content;
using CardWars.Client.scripts.core.profile;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Core.Storage;
using CardWars.Server;
using CardWars.Server.Listener;
using Godot;

namespace CardWars.Client.scripts.core.application;

public sealed class SessionRuntime
{
	private const string DefaultSessionName = "session_1";

	private readonly GameApplication _app;
	private readonly Node _root;
	private readonly string _username;
	private readonly string _targetIpAddress;
	private readonly int _targetPort;

	private SessionContext? _context;
	private SessionStorage? _storage;
	private GameContent? _content;
	private BattleEngineRegistry? _battleRegistry;
	private LocalListener? _localListener;

	public SessionRuntime(
		GameApplication app,
		Node root,
		string username,
		SessionMode mode,
		string targetIpAddress,
		int targetPort)
	{
		_app = app;
		_root = root;
		_username = username;
		Mode = mode;
		_targetIpAddress = targetIpAddress;
		_targetPort = targetPort;
	}

	public SessionMode Mode { get; }
	public SessionContext Context => _context ?? throw new InvalidOperationException("Session has not started.");
	public ClientRuntime? Client { get; private set; }
	public ServerRuntime? Server { get; private set; }

	public Action<string>? OnDebugStatus { get; set; }
	public Action<string>? OnDebugWorld { get; set; }
	public Action<string>? OnDebugPlayers { get; set; }

	private bool HasClient => Mode is SessionMode.Client or SessionMode.Integrated;
	private bool HasServer => Mode is SessionMode.Integrated or SessionMode.Server;

	public void Start()
	{
		if (_context is not null)
			throw new InvalidOperationException("Session has already started.");

		// Get all content (battle/server/client)
		_content = GameContent.Load(_app.Mods);

		// Load up battle mods and common battle registry
		_battleRegistry = new BattleEngineRegistry(_app.DataTags);
		foreach (var mod in _app.Mods.LoadModEntry<IBattleEngineMod>())
			mod.OnLoad(_battleRegistry, _content.BattleData);

		// Server storage
		if (HasServer)
			_storage = _app.Storage.OpenSession(DefaultSessionName);
		_context = new SessionContext(_app.DataTags, _storage, _content, _battleRegistry);

		if (HasServer) StartServer();
		if (HasClient) StartClient(); // start client after server is up (TODO: although we should really do a timer request and not immediately quit...)

		Log.Info($"Session started in {Mode} mode with username: {_username}");
	}

	private void StartClient()
	{

		// Determine connection type (integrated or connection)
		IConnection connection;
		if (Mode == SessionMode.Integrated)
			connection = _localListener?.ConnectClient()
				?? throw new InvalidOperationException("An integrated client requires a local listener.");
		else
			connection = new TcpConnection(
				new TcpClient(_targetIpAddress, _targetPort),
				new PacketCodec(Context.DataTags));

		// Prep client mdos
		var clientMods = _app.CreateModEntries<IClientMod>()
			.Concat(_app.Mods.LoadModEntry<IClientMod>())
			.ToList();

		// Profile store (cuz its different from `Context.Storage`? TODO: Maybe combine them into one)
		var profileRoot = new StoragePath(
			_app.Provider.Combine(_app.GamedataPath, ".userdata"),
			_app.Provider);
		var profiles = new ClientProfileStore(profileRoot, Context.DataTags);

		Client = new ClientRuntime(
			_root,
			Context,
			_username,
			profiles,
			clientMods,
			connection)
		{
			OnDebugStatus = value => OnDebugStatus?.Invoke(value),
			OnDebugWorld = value => OnDebugWorld?.Invoke(value),
			OnDebugPlayers = value => OnDebugPlayers?.Invoke(value)
		};
		Client.Start();
	}

	private void StartServer()
	{
		// Setup all listeners
		var listeners = new List<IListener>();
		var codec = new PacketCodec(Context.DataTags);
		_localListener = null;

		if (Mode == SessionMode.Integrated)
		{
			_localListener = new LocalListener(codec) { IsSerialized = true };
			listeners.Add(_localListener);
		}

		listeners.Add(new TcpGameListener(codec, _targetPort));

		// Prep server mods
		var serverMods = _app.Mods.LoadModEntry<IServerMod>().ToList();

		Server = new ServerRuntime(Context, serverMods, listeners);
		Server.Start();
	}

	public void Process() => Client?.Process();

	public void Stop()
	{
		Client?.Stop();
		Server?.Stop();
		_app.Storage.CloseSession();
	}
}
