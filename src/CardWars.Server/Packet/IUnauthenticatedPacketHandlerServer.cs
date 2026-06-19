using CardWars.Core.Network.Packet;
using CardWars.Core.Request;

namespace CardWars.Server.Packet;

public interface IUnauthenticatedPacketHandlerServer<TPacket> : IRequestHandler<PacketUnauthenticatedContextServer, TPacket>
	where TPacket : IPacket;