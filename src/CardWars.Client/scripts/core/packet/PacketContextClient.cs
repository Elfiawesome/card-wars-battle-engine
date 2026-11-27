using System;
using CardWars.Client.scripts.core.application;
using CardWars.Core.Network.Transport;

namespace CardWars.Client.scripts.core.packet;

public record struct PacketContextClient(
	ClientRuntime Runtime
)
{
	public readonly IConnection Connection => Runtime.Connection ?? throw new InvalidOperationException("Client is not connected.");
};