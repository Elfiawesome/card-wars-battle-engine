using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler(WorldRegistry worldRegistry, string worldKind) : IUnauthenticatedPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketUnauthenticatedContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
		var persistentId = context.Server.Session.UsernameToPlayerId(request.Username);
		if (persistentId == Guid.Empty)
		{
			persistentId = Guid.NewGuid();
			context.Server.Session.SaveUsernameMapping(request.Username, persistentId);
			Logger.Info("Mapping does not exist, making one now");
		}

		PlayerSession playerSession;
		var data = context.Server.Session.LoadPlayer(persistentId);
		if (data != null)
		{
			// Existing player & load data
			playerSession = DataTagMapper.FromTag<PlayerSession>(data);
			playerSession.Connection = context.Connection;
		}
		else
		{
			// New player & create new data -> save
			playerSession = new PlayerSession() { Connection = context.Connection, PlayerId = persistentId };
			context.Server.Session.SavePlayer(persistentId, DataTagMapper.ToTag(playerSession, false));
		}

		context.Server.RemoveUnauthenticatedConnection(context.Connection);
		context.Server.AddPlayer(playerSession);

		var world = context.Server.GetOrCreateInstance(worldKind, worldRegistry.DefaultWorld.ToFlatString());
		context.Server.AssignPlayerToInstance(playerSession, world);

		playerSession.Connection.Send(new S2C_ConnectionConfirmedPacket() { Message = $"Welcome, {playerSession.Username}!" });
		Logger.Info($"[{playerSession.Username}] [{playerSession.PlayerId}] has connected!");
	}
}
