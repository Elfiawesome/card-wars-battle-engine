using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Client.scripts.cardwars.packet;

public class S2C_PlayerJoinedRequestPacketHandler() : IPacketHandlerClient<S2C_PlayerJoinedRequestPacket>
{
	public void Handle(PacketContextClient context, S2C_PlayerJoinedRequestPacket request)
	{
		context.Connection.Send(new C2S_PlayerJoinedRequestResponsePacket()
		{
			Username = context.Runtime.ConnectingUsername,
			Profile = context.Runtime.Profiles.Load(context.Runtime.ConnectingUsername)
		});

		context.Runtime.SetDebugStatus("Sent player info, waiting for confirmation...");
	}
}

public class S2C_ConnectionConfirmedPacketHandler : IPacketHandlerClient<S2C_ConnectionConfirmedPacket>
{
	public void Handle(PacketContextClient context, S2C_ConnectionConfirmedPacket request)
	{
		context.Runtime.SetActiveProfile(request.Profile);
		if (request.Profile is { } profile)
			context.Runtime.Profiles.Save(context.Runtime.ConnectingUsername, profile);

		context.Runtime.SetDebugStatus($"Connected! {request.Message}");
	}
}

public class S2C_EnterInstancePacketHandler : IPacketHandlerClient<S2C_EnterInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_EnterInstancePacket request)
	{
		var instance = context.Runtime.Registry.Instances.Instantiate<ClientInstance>(request.ProviderId);
		if (instance is null) { return; }
		context.Runtime.SwitchInstance(instance, request.PlayerId);
	}
}

public class S2C_EnterInstanceRejectedPacketHandler : IPacketHandlerClient<S2C_EnterInstanceRejectedPacket>
{
	public void Handle(PacketContextClient context, S2C_EnterInstanceRejectedPacket request)
	{
		context.Runtime.SetDebugStatus($"Cannot enter {request.ProviderId}: {request.Reason}");
	}
}

public class S2C_LeaveInstancePacketHandler : IPacketHandlerClient<S2C_LeaveInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_LeaveInstancePacket request)
	{
		context.Runtime.SetDebugStatus($"Left instance {request.PlayerId}");
	}
}

