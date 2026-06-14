using CardWars.Core.Network.Packet;
using CardWars.Server.Packet;

namespace CardWars.Server.Vanilla.Packet;

public class C2S_CustomModPacketHandler : IPacketHandlerServer<C2S_CustomModPacket>
{
	public void Handle(PacketContextServer context, C2S_CustomModPacket request)
	{
		// TODO
	}
}