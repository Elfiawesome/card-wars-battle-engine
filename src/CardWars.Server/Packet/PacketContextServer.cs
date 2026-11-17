using CardWars.Core.Logging;
using CardWars.Server.Session;

namespace CardWars.Server.Packet;

public record struct PacketContextServer(
	Server Server,
	PlayerSession PlayerSession
)
{
	/// <summary>The owning server's logger, so packet handlers log through the session logger.</summary>
	public readonly ILogger Logger => Server.Logger;
}