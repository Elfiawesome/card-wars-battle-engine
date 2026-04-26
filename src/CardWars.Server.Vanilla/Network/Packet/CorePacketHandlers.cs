using CardWars.Core.Network.Packet;
using CardWars.Server.Packet;

namespace CardWars.Server.Vanilla.Network.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler : IPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
		context.PlayerSession.PlayState = Session.PlayState.Play;
	}
}


public class C2S_CustomModPacketHandler : IPacketHandlerServer<C2S_CustomModPacket>
{
	public void Handle(PacketContextServer context, C2S_CustomModPacket request)
	{
		// TODO
	}
}