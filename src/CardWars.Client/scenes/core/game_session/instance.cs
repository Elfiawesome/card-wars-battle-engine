
using System;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class ClientInstance : Node
{
	public Action<IPacket>? onPacketSent;
	public virtual void OnPacket(IPacket packet, PacketContextClient context) { }

	public void SendPacket(IPacket packet)
		=> onPacketSent?.Invoke(packet);
}