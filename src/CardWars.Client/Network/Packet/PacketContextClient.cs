using System;
using CardWars.Core.Network.Transport;

namespace CardWars.Client;

public record struct PacketContextClient(
	GameSession Session
)
{
	public readonly IConnection Connection => Session.Connection ?? throw new Exception();
};