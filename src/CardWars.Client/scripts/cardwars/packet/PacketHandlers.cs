using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Client.scripts.cardwars.packet;

public class S2C_PlayerJoinedRequestPacketHandler() : IPacketHandlerClient<S2C_PlayerJoinedRequestPacket>
{
	public void Handle(PacketContextClient context, S2C_PlayerJoinedRequestPacket request)
	{
		context.Connection.Send(new C2S_PlayerJoinedRequestResponsePacket()
		{
			Username = context.Session.ConnectingUsername,
			Profile = context.Session.Profiles.Load(context.Session.ConnectingUsername)
		});

		context.Session.SetDebugStatus("Sent player info, waiting for confirmation...");
	}
}

public class S2C_ConnectionConfirmedPacketHandler : IPacketHandlerClient<S2C_ConnectionConfirmedPacket>
{
	public void Handle(PacketContextClient context, S2C_ConnectionConfirmedPacket request)
	{
		context.Session.SetActiveProfile(request.Profile);
		if (request.Profile is { } profile)
			context.Session.Profiles.Save(context.Session.ConnectingUsername, profile);

		context.Session.SetDebugStatus($"Connected! {request.Message}");
	}
}

public class S2C_EnterInstancePacketHandler : IPacketHandlerClient<S2C_EnterInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_EnterInstancePacket request)
	{
		var instance = context.Session.Registry.Instances.Instantiate<ClientInstance>(request.ProviderId);
		if (instance is null) { return; }
		context.Session.SwitchInstance(instance, request.PlayerId);
	}
}

public class S2C_LeaveInstancePacketHandler : IPacketHandlerClient<S2C_LeaveInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_LeaveInstancePacket request)
	{
		context.Session.SetDebugStatus($"Left instance {request.PlayerId}");
	}
}

