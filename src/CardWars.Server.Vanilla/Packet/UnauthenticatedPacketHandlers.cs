using CardWars.Core.Data.Mapping;
using CardWars.Core.Logging;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Server.Vanilla.Session;
using CardWars.Vanilla.Shared;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler(PlayerProfileRegistry profileRegistry) : IUnauthenticatedPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketUnauthenticatedContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
		var persistentId = context.Server.Session.UsernameToPlayerId(request.Username);
		if (persistentId == Guid.Empty)
		{
			persistentId = Guid.NewGuid();
			context.Server.Session.SaveUsernameMapping(request.Username, persistentId);
			Log.Info("Mapping does not exist, making one now");
		}

		PlayerSession playerSession;

		var data = context.Server.Session.LoadPlayer(persistentId);
		if (data is not null)
		{
			playerSession = DataTagMapper.FromTag<PlayerSession>(data);
			playerSession.Connection = context.Connection;
		}
		else
		{
			playerSession = new() { Connection = context.Connection, PlayerId = persistentId };
			context.Server.Session.SavePlayer(persistentId, DataTagMapper.ToTag(playerSession, false));
		}

		playerSession.Username = request.Username;

		// Get the server's profile or use client's profile
		var profileData = playerSession.GetSetComponent<ProfilePlayerDataComponent>(SharedIds.PlayerData.Profile);
		if (profileRegistry.Authority == ProfileAuthority.Client)
		{
			if (request.Profile is { } clientProfile)
			{
				profileData.Profile = clientProfile.Copy();
				Log.Info($"Accepted client profile for [{playerSession.Username}]");
			}
			else
			{
				Log.Warn($"Client profile missing for [{playerSession.Username}], using server profile");
			}
		}

		if (profileData.Profile is null && profileRegistry.Starter is { } starter)
			profileData.Profile = starter.Copy();

		playerSession.Connection.Send(new S2C_ConnectionConfirmedPacket()
		{
			Message = $"Welcome, {playerSession.Username}!",
			Profile = profileData.Profile?.Copy()
		});

		context.Server.RemoveUnauthenticatedConnection(context.Connection);
		context.Server.AddPlayer(playerSession);

		Log.Info($"[{playerSession.Username}] [{playerSession.PlayerId}] has connected!");
	}
}