using System;
using System.Collections.Generic;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.cardwars.registry;
using CardWars.Client.scripts.core.content;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.Runtime.Application;
using CardWars.Client.Runtime.Profile;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.core.application;

public sealed class ClientRuntime
{
	private readonly Node _root;
	private readonly IReadOnlyList<IClientMod> _mods;
	private readonly IConnection _connection;
	private bool _started;

	public ClientRuntime(
		Node root,
		SessionContext context,
		string connectingUsername,
		ClientProfileStore profiles,
		IReadOnlyList<IClientMod> mods,
		IConnection connection)
	{
		_root = root;
		_mods = mods;
		_connection = connection;
		Context = context;
		ConnectingUsername = connectingUsername;
		Profiles = profiles;

		Registry = new ClientRegistry();
		Registry.RegisterExtension(new BattleClientRuntime(Context.BattleRegistry));
	}

	public SessionContext Context { get; }
	public ClientRegistry Registry { get; }
	public ClientProfileStore Profiles { get; }
	public string ConnectingUsername { get; }
	public PlayerProfile? ActiveProfile { get; private set; }

	public IConnection? Connection { get; private set; }
	public ClientInstance? Instance { get; private set; }

	public Action<string>? OnDebugStatus { get; set; }
	public Action<string>? OnDebugWorld { get; set; }
	public Action<string>? OnDebugPlayers { get; set; }

	public void Start()
	{
		if (_started)
			throw new InvalidOperationException("Client runtime has already started.");

		// Load all contents
		var contentLoader = new ClientContentLoader(Registry.Resources, Context.Logger);
		contentLoader.MountPacks(Context.Content.ClientPacks);
		contentLoader.RegisterMountedAssets();
		contentLoader.LoadRawAssets(Context.Content.BattleAssets);

		// Load all mods
		foreach (var mod in _mods)
			mod.OnLoad(Registry, Context.Content.ClientData);

		Connection = _connection;
		_started = true;
	}

	public void Process()
	{
		if (Connection is { IsConnected: true } connection)
		{
			while (connection.TryReceive(out var packet))
			{
				if (packet is { } received) HandleIncomingPacket(received);
			}
		}
	}

	public void SwitchInstance(ClientInstance newInstance, Guid? playerId)
	{
		if (Instance is { } oldInstance)
			_root.RemoveChild(oldInstance);

		Instance = newInstance;
		newInstance.Initialize(new ClientInstanceContext(
			new ClientInstanceRegistry(Registry),
			playerId ?? Guid.Empty,
			SendPacket,
			Context.Logger));
		_root.AddChild(newInstance);
	}

	public void SendPacket(IPacket packet) => Connection?.Send(packet);

	public void SetActiveProfile(PlayerProfile? profile) => ActiveProfile = profile;

	public void SetDebugStatus(string value) => OnDebugStatus?.Invoke(value);

	public void SetDebugWorld(string value) => OnDebugWorld?.Invoke(value);

	public void SetDebugPlayers(string value) => OnDebugPlayers?.Invoke(value);

	public void Stop()
	{
		Connection?.Disconnect();
		Connection = null;
		_started = false;
	}

	private void HandleIncomingPacket(IPacket packet)
	{
		Context.Logger.Debug($"Client received packet from server: {packet.GetType().Name}");
		var context = new PacketContextClient(this);
		if (Instance is { } instance && instance.OnPacket(packet, context)) return;
		Registry.PacketHandlers.Execute(context, packet);
	}
}
