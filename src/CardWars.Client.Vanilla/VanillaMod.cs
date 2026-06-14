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
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler());
		registry.PacketHandlers.Register(new S2C_ConnectionConfirmedPacketHandler());

		DataTagTypeRegistry.ScanAssembly(typeof(S2C_PlayerJoinedRequestPacket).Assembly);
	}
}
