using System;
using CardWars.Client.scenes.core.game_session;
using CardWars.Core.Network.Transport;

namespace CardWars.Client.scripts.core.packet;

public record struct PacketContextClient(
	ClientSession Session
)
{
	public readonly IConnection Connection => Session.Connection ?? throw new InvalidOperationException("Client is not connected.");
};