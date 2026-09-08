using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Client.scripts.vanilla.packet;

public class S2C_PlayerJoinedRequestPacketHandler() : IPacketHandlerClient<S2C_PlayerJoinedRequestPacket>
{
	public void Handle(PacketContextClient context, S2C_PlayerJoinedRequestPacket request)
	{
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
		var instance = context.Session.ClientRegistry.Instances.InstantiateInstance<ClientInstance>(request.ProviderId);
		if (instance == null) { return; }
		context.Session.SwitchInstance(instance);
	}
}

public class S2C_LeaveInstancePacketHandler : IPacketHandlerClient<S2C_LeaveInstancePacket>
{
	public void Handle(PacketContextClient context, S2C_LeaveInstancePacket request)
	{
		context.Session.SetDebugStatus($"Left instance {request.PlayerId}");
	}
}

public class S2C_BattleBlockBatchHandler : IPacketHandlerClient<S2C_BattleBlockBatch>
{
	public void Handle(PacketContextClient context, S2C_BattleBlockBatch request)
	{
		context.Session.HandleBattleBlockBatch(request.Batch);
	}
}

// Custom Handler
public class S2C_CustomModPacketHandler : IPacketHandlerClient<S2C_CustomModPacket>
{
	public void Handle(PacketContextClient context, S2C_CustomModPacket request) { /* TODO */ }
}
