using Logger = CardWars.Core.Logging.Logger;
using System;
using CardWars.Client.scripts.core;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using Godot;

namespace CardWars.Client.scenes.core.game_session;

public partial class ClientInstance : Node
{
	private ClientInstanceContext? _context;

	public ClientInstanceContext Context => _context
		?? throw new InvalidOperationException("Client instance has not been initialized.");

	public ClientInstanceRegistry Registry => Context.Registry;

	public Guid MyPlayerId => Context.PlayerId;
	public Logger Logger => Context.Logger;

	public void Initialize(ClientInstanceContext context)
	{
		if (_context is not null)
			throw new InvalidOperationException("Client instance has already been initialized.");
		_context = context;
	}

	public virtual bool OnPacket(IPacket packet, PacketContextClient context) => false;

	public void SendPacket(IPacket packet) => Context.SendPacket(packet);
}

public readonly record struct ClientInstanceContext(
	ClientInstanceRegistry Registry,
	Guid PlayerId,
	Action<IPacket> SendPacket,
	Logger Logger
);

public record struct ClientInstanceRegistry(
	// TODO: Legacy, we removed battle registry and now just have this wrapper
	ClientRegistry? Client
);
