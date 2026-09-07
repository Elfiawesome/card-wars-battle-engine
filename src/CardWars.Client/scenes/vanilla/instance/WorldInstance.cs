using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance;

public partial class WorldInstance : ClientInstance
{
	public override void OnPacket(IPacket packet, PacketContextClient context)
	{
		switch (packet)
		{
			case S2C_WorldInstanceSnapshot worldInstanceSnapshot:
				// TODO:
				// context.Session.SetDebugStatus(...);
				// context.Session.SetDebugWorld(...);
				// context.Session.SetDebugPlayers(...);
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		// C2S_MoveInputPacket
	}

}
