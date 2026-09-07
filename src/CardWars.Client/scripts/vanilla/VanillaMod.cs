using System.Collections.Generic;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.vanilla.packet;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared;

namespace CardWars.Client.scripts.vanilla;

public class VanillaMod : IClientMod
{
	public string ModName => throw new System.NotImplementedException();
	public string Version => throw new System.NotImplementedException();

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new S2C_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler());
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_LeaveInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_BattleBlockBatchHandler());

		registry.Instances.Register(Constant.WorldInstanceId, "res://scenes/vanilla/instance/world/world_instance.tscn");
		registry.Instances.Register(Constant.BattleInstanceId, "res://scenes/vanilla/instance/battle/battle_instance.tscn");
	}
}