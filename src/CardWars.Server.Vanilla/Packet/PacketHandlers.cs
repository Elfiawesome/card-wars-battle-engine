using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler(WorldRegistry worldRegistry) : IPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
		var alreadyOnline = context.Server.PlayerSessions.Values
			.Any(p => p.Username == request.Username);

		if (alreadyOnline)
		{
			context.PlayerSession.Connection.Disconnect();
			return;
		}

		var persistentId = context.Server.Session.UsernameToPlayerId(request.Username);
		if (persistentId == null)
		{
			// New player
			context.Server.Session.SaveUsernameMapping(request.Username, context.PlayerSession.PlayerId);
			context.PlayerSession.Username = request.Username;
			context.Server.Session.SavePlayer(context.PlayerSession.PlayerId, context.PlayerSession.PersistentData);
		}
		else
		{
			// Returning player
			var save = context.Server.Session.LoadPlayer((Guid)persistentId);
			context.PlayerSession.PersistentData = save != null ? (CompoundTag)save : new();
			context.PlayerSession.Username = request.Username;
			context.Server.SwapPlayerSessionIds(context.PlayerSession.PlayerId, (Guid)persistentId);
		}

		// Set player to their instance
		// TODO

		context.PlayerSession.PlayState = PlayState.Play;
		context.PlayerSession.Connection.Send(new S2C_ConnectionConfirmedPacket()
		{
			Message = $"Welcome, {context.PlayerSession.Username}!"
		});
		Logger.Info($"[{context.PlayerSession.Username}] [{context.PlayerSession.PlayerId}] has connected!");
	}
}


public class C2S_CustomModPacketHandler : IPacketHandlerServer<C2S_CustomModPacket>
{
	public void Handle(PacketContextServer context, C2S_CustomModPacket request)
	{
		// TODO
	}
}