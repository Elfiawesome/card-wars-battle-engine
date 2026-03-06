using CardWars.Core.Request;

namespace CardWars.Server.Packet;

public interface IPacketHandler<TPacket> : IRequestHandler<PacketContext, TPacket>
	where TPacket : IPacket;
