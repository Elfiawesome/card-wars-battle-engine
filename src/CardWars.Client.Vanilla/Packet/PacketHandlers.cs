using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Client.Vanilla.Packet;

public class S2C_PlayerJoinedRequestPacketHandler : IPacketHandlerClient<S2C_PlayerJoinedRequestPacket>
{
	public void Handle(PacketContextClient context, S2C_PlayerJoinedRequestPacket request)
	{
		context.Connection.Send(new C2S_PlayerJoinedRequestResponsePacket()
		{
			Username = context.Session.ConnectingUsername
		});
	}
}

public class C2S_CustomModPacketHandler : IPacketHandlerClient<S2C_CustomModPacket>
{
	public void Handle(PacketContextClient context, S2C_CustomModPacket request)
	{
		// TODO
	}
}