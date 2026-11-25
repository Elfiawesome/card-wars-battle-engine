
using System;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class ClientInstance : Node
{
	public Guid MyPlayerId = Guid.Empty;
	public Action<IPacket>? onPacketSent;
	public ClientInstanceRegistry Registry = new();

	public virtual bool OnPacket(IPacket packet, PacketContextClient context) => false;

	public void SendPacket(IPacket packet)
		=> onPacketSent?.Invoke(packet);
}

public record struct ClientInstanceRegistry(
	ClientRegistry? Client
);
