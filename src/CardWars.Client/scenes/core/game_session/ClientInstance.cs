
using System;
using CardWars.BattleEngine;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class ClientInstance : Node
{
	public Guid MyPlayerId = Guid.Empty;
	public Action<IPacket>? onPacketSent;
	public ClientRegistry? ClientRegistry;
	public BattleEngineRegistry? BattleEngineRegistry;

	public virtual void OnPacket(IPacket packet, PacketContextClient context) { }

	public void SendPacket(IPacket packet)
		=> onPacketSent?.Invoke(packet);
}