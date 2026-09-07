using System.Collections.Generic;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.vanilla.packet;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using Godot;

namespace CardWars.Client.scripts.vanilla;

public class VanillaMod : IClientMod
{
	public string ModName => throw new System.NotImplementedException();
	public string Version => throw new System.NotImplementedException();

	private WorldClientState _world = new();

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new S2C_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler(_world));
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_LeaveInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_WorldSnapshotPacketHandler(_world));
		registry.PacketHandlers.Register(new S2C_BattleBlockBatchHandler());

		registry.Instances.Register(ResourceId.Vanilla("world"), "res://scenes/vanilla/instance/world_instance.tscn");
	}
}