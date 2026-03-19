using CardWars.Core.Network.Packet;
using CardWars.Core.Request;

namespace CardWars.Server.Packet;

public interface IPacketHandlerServer<TPacket> : IRequestHandler<PacketContextServer, TPacket>
	where TPacket : IPacket;
