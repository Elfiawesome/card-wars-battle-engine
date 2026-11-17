
using System;
using CardWars.BattleEngine;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Data;
using CardWars.Core.Network.Packet;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class ClientInstance : Node
{
	public Guid MyPlayerId = Guid.Empty;
	public Action<IPacket>? onPacketSent;
	// public ClientRegistry? Registry.ClientRegistry;
	// public BattleEngineRegistry? BattleEngineRegistry;
	public ClientInstanceRegistry Registry = new();

	public virtual void OnPacket(IPacket packet, PacketContextClient context) { }

	public void SendPacket(IPacket packet)
		=> onPacketSent?.Invoke(packet);
}

public record struct ClientInstanceRegistry(
	ClientRegistry? Client,
	BattleEngineRegistry? Battle,
	// The owning session's DataContext (Phase 1/5): threaded in so instance-level validators and
	// (de)serialization run against the session-scoped context rather than a static facade.
	DataContext? Data
);