using CardWars.Server.Vanilla.Network.Packet;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	public void OnLoad(ServerRegistry registry)
	{
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler());
	}
}