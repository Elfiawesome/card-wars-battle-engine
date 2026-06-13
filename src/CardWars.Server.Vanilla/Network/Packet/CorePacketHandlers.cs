using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Server.Packet;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Network.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler : IPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
		var persistentId = context.Server.Session.UsernameToPlayerId(request.Username);
		if (persistentId == null)
		{
			// New player
			context.Server.Session.SetUsernameFromPlayerId(request.Username, context.PlayerSession.PlayerId);
			context.PlayerSession.Username = request.Username;
			// var w = new WorldInstance(Guid.NewGuid());
			
			// context.Server.AddPlayerToInstance(context.PlayerSession.PlayerId, context.Server.Registry.DefaultWorld)
			
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
		context.PlayerSession.PlayState = Session.PlayState.Play;
	}
}


public class C2S_CustomModPacketHandler : IPacketHandlerServer<C2S_CustomModPacket>
{
	public void Handle(PacketContextServer context, C2S_CustomModPacket request)
	{
		// TODO
	}
}