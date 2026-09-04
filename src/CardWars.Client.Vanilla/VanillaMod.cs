using CardWars.Client.Vanilla.Packet;
using CardWars.ModLoader;

namespace CardWars.Client.Vanilla;

public class VanillaMod : IClientMod
{
	public string ModName => "Vanilla";

	public string Version => "";

	private readonly WorldClientState _world = new();

	public void OnLoad(ClientRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new S2C_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler(_world));
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());
		registry.PacketHandlers.Register(new S2C_EnterInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_LeaveInstancePacketHandler());
		registry.PacketHandlers.Register(new S2C_WorldSnapshotPacketHandler(_world));
		registry.PacketHandlers.Register(new S2C_BattleBlockBatchHandler());
	}
}
