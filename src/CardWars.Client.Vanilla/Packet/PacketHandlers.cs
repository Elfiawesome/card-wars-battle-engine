using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Client.Vanilla.Packet;

public class S2C_PlayerJoinedRequestPacketHandler(WorldClientState world) : IPacketHandlerClient<S2C_PlayerJoinedRequestPacket>
{
	public void Handle(PacketContextClient context, S2C_PlayerJoinedRequestPacket request)
	{
		world.Hook(context.Session);

		context.Connection.Send(new C2S_PlayerJoinedRequestResponsePacket()
		{
			Username = context.Session.ConnectingUsername
		});

		context.Session.SetDebugStatus("Sent player info, waiting for confirmation...");
	}
}

public class S2C_ConnectionConfirmedPacketHandler : IPacketHandlerClient<S2C_ConnectionConfirmedPacket>
{
	public void Handle(PacketContextClient context, S2C_ConnectionConfirmedPacket request)
	{
		context.Session.SetDebugStatus($"Connected! {request.Message}");
	}
}

public class S2C_EnterInstancePacketHandler : IPacketHandlerClient<S2C_EnterInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_EnterInstancePacket request)
	{
		context.Session.SetDebugStatus($"Entered instance {request.PlayerId}");
	}
}

public class S2C_LeaveInstancePacketHandler : IPacketHandlerClient<S2C_LeaveInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_LeaveInstancePacket request)
	{
		context.Session.SetDebugStatus($"Left instance {request.PlayerId}");
	}
}

public class S2C_WorldSnapshotPacketHandler(WorldClientState world) : IPacketHandlerClient<S2C_WorldInstanceSnapshot>
{
	public void Handle(PacketContextClient context, S2C_WorldInstanceSnapshot request)
	{
		world.OnSnapshot(request.WorldView, context.Session);
		context.Session.SetDebugStatus($"Received snapshot for world {request.WorldView.WorldId}");
	}
}

// Custom Handler

public class S2C_CustomModPacketHandler : IPacketHandlerClient<S2C_CustomModPacket>
{
	public void Handle(PacketContextClient context, S2C_CustomModPacket request) { /* TODO */ }
}
