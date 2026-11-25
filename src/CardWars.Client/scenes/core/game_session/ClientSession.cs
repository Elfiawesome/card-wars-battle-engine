using System;
using System.Collections.Generic;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.content;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.core.profile;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Core.Network.Transport;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public sealed class ClientSession(Node root, string connectingUsername, ClientRegistry registry, ClientProfileStore profiles)
{
	public ClientRegistry Registry { get; } = registry;
	public ClientProfileStore Profiles { get; } = profiles;
	public string ConnectingUsername { get; } = connectingUsername;
	public PlayerProfile? ActiveProfile { get; private set; }

	public IConnection? Connection { get; private set; }
	public ClientInstance? Instance { get; private set; }

	public Action<string>? OnDebugStatus { get; set; }
	public Action<string>? OnDebugWorld { get; set; }
	public Action<string>? OnDebugPlayers { get; set; }

	public void LoadContent(
		IEnumerable<IClientMod> mods,
		IReadOnlyList<ModContentResult> clientPacks,
		IReadOnlyList<ModContentResult> clientData,
		IReadOnlyList<ModContentResult> rawAssets)
	{
		var contentLoader = new ClientContentLoader(Registry.Resources);
		contentLoader.MountPacks(clientPacks);
		contentLoader.RegisterMountedAssets();
		contentLoader.LoadRawAssets(rawAssets);

		foreach (var mod in mods)
			mod.OnLoad(Registry, clientData);
	}

	public void Connect(IConnection connection) => Connection = connection;

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
		{
			root.RemoveChild(oldInstance);
			oldInstance.onPacketSent -= SendPacket;
		}

		Instance = newInstance;
		Instance.onPacketSent += SendPacket;
		Instance.Registry = new ClientInstanceRegistry(Registry);
		Instance.MyPlayerId = playerId ?? Guid.Empty;
		root.AddChild(Instance);
	}

	public void SendPacket(IPacket packet) => Connection?.Send(packet);

	public void SetActiveProfile(PlayerProfile? profile) => ActiveProfile = profile;

	public void Disconnect() => Connection?.Disconnect();

	public void SetDebugStatus(string value) => OnDebugStatus?.Invoke(value);

	public void SetDebugWorld(string value) => OnDebugWorld?.Invoke(value);

	public void SetDebugPlayers(string value) => OnDebugPlayers?.Invoke(value);

	private void HandleIncomingPacket(IPacket packet)
	{
		Log.Debug($"Client received packet from server: {packet.GetType().Name}");
		var context = new PacketContextClient(this);
		if (Instance is { } instance && instance.OnPacket(packet, context)) return;
		Registry.PacketHandlers.Execute(context, packet);
	}
}
