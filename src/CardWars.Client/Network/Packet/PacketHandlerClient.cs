using CardWars.Core.Network.Packet;
using CardWars.Core.Request;

namespace CardWars.Client;

public interface IPacketHandlerClient<TPacket> : IRequestHandler<PacketContextClient, TPacket>
	where TPacket : IPacket;
