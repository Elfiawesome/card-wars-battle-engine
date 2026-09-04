using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Server.Packet;
using CardWars.Server.Vanilla.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Packet;

public class C2S_CustomModPacketHandler : IPacketHandlerServer<C2S_CustomModPacket>
{
	public void Handle(PacketContextServer context, C2S_CustomModPacket request)
	{
		// TODO
	}
}

public class C2S_DEBUG_WarpRequestPacketHandler : IPacketHandlerServer<C2S_DEBUG_WarpRequestPacket>
{
	public void Handle(PacketContextServer context, C2S_DEBUG_WarpRequestPacket request)
	{
		if (context.PlayerSession.CurrentInstance is not WorldInstance world) return;

		if (!world.GetWarpOptions().Contains(request.TargetWorld))
		{
			Logger.Warn($"Player tried to warp to invalid world '{request.TargetWorld}' from '{world.WorldId}'.");
			return;
		}

		context.Server.EnterInstance(context.PlayerSession, world.InstanceProviderId, request.TargetWorld.ToString());
	}
}
