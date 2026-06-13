using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Server;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.Network.Packet;

namespace CardWars.Server.Vanilla.Network.Packet;

public class C2S_PlayerJoinedRequestResponsePacketHandler(WorldRegistry worldRegistry) : IPacketHandlerServer<C2S_PlayerJoinedRequestResponsePacket>
{
	public void Handle(PacketContextServer context, C2S_PlayerJoinedRequestResponsePacket request)
	{
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

		AssignPlayerToWorld(context);

		context.PlayerSession.PlayState = Session.PlayState.Play;
	}

	private void AssignPlayerToWorld(PacketContextServer context)
	{
		var template = worldRegistry.Templates.Get(worldRegistry.DefaultWorld);
		if (template == null) return;

		var instance = new WorldInstance(Guid.NewGuid());
		instance.Load(template);
		context.Server.CreateInstance(instance);
		context.Server.AddPlayerToInstance(context.PlayerSession, instance);
	}
}


public class C2S_CustomModPacketHandler : IPacketHandlerServer<C2S_CustomModPacket>
{
	public void Handle(PacketContextServer context, C2S_CustomModPacket request)
	{
		// TODO
	}
}