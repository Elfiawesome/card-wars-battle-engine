using CardWars.Server.Vanilla.Network.Packet;
using CardWars.ModLoader;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	public void OnLoad(ServerRegistry registry, List<ModContentResult> modContents)
	{
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler());
	}
}