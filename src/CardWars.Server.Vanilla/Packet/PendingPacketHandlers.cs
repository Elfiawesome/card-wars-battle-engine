using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler(WorldRegistry worldRegistry) : IPendingPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketPendingContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
		// Check for duplicates
		var alreadyOnline = context.Server.PlayerSessions.Values
			.Any(p => p.Username == request.Username);

		if (alreadyOnline)
		{
			context.Connection.Disconnect();
			return;
		}

		PlayerSession? playerSession = null;
		var persistentId = context.Server.Session.UsernameToPlayerId(request.Username);
		if (persistentId == null)
		{
			// New player (Need to save mapping)
			playerSession = new PlayerSession() { Connection = context.Connection, PlayerId = Guid.NewGuid() };
			context.Server.Session.SaveUsernameMapping(request.Username, playerSession.PlayerId);
		}
		else
		{
			// Returning player
			var save = context.Server.Session.LoadPlayer((Guid)persistentId);
			if (save is CompoundTag saveCompoundTag)
			{
				playerSession = DataTagMapper.FromTag<PlayerSession>(saveCompoundTag);
			}
		}

		if (playerSession == null) { return; }
		// Set player data from client and add them
		playerSession.Username = request.Username;
		context.Server.AddPlayer(playerSession);

		// TODO: Set player to their instance

		playerSession?.Connection.Send(new S2C_ConnectionConfirmedPacket()
		{
			Message = $"Welcome, {playerSession.Username}!"
		});
		
		Logger.Info($"[{playerSession?.Username}] [{playerSession?.PlayerId}] has connected!");
	}
}