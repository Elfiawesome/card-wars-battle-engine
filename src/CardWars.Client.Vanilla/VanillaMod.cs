using CardWars.Client.Vanilla.Packet;
using CardWars.Core.Data;
using CardWars.ModLoader;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Client.Vanilla;

public class VanillaMod : IClientMod
{
	public string ModName => "Vanilla";

	public string Version => "";

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new S2C_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler());
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_LeaveInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_WorldSnapshotPacketHandler());
	}
}
