using CardWars.Core.Network.Packet;
using CardWars.Core.Request;

namespace CardWars.Client.scripts.core.packet;

public interface IPacketHandlerClient<TPacket> : IRequestHandler<PacketContextClient, TPacket>
	where TPacket : IPacket;
