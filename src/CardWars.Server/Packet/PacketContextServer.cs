using CardWars.Core.Network.Transport;
using CardWars.Server.Session;

namespace CardWars.Server.Packet;

public record struct PacketContextServer(
	Server Server,
	PlayerSession PlayerSession
);

public record struct PacketPendingContextServer(
	Server Server,
	IConnection Connection
);