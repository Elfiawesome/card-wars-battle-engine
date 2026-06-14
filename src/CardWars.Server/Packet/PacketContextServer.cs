using CardWars.Server.Session;

namespace CardWars.Server.Packet;

public record struct PacketContextServer(
	Server Server,
	PlayerSession PlayerSession
);