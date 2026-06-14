using CardWars.Core.Network.Packet;
using CardWars.Core.Request;

namespace CardWars.Server.Packet;

public interface IPendingPacketHandlerServer<TPacket> : IRequestHandler<PacketPendingContextServer, TPacket>
	where TPacket : IPacket;