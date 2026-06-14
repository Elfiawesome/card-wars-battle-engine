using CardWars.Core.Network.Transport;

namespace CardWars.Server.Packet;

public record struct PacketPendingContextServer(
	Server Server,
	IConnection Connection
);