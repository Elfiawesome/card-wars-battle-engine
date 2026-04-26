using CardWars.Client.Vanilla.Network.Packet;

namespace CardWars.Client.Vanilla;

public class VanillaMod : IClientMod
{
	public string ModName => "Vanilla";

	public string Version => "";

	public void OnLoad(ClientRegistry registry)
	{
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new S2C_PlayerJoinedRequestPacketHandler());
	}
}