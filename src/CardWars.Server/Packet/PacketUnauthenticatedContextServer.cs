using CardWars.Core.Network.Transport;

namespace CardWars.Server.Packet;

public record struct PacketUnauthenticatedContextServer(
	Server Server,
	IConnection Connection
);