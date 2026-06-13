using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Server;
using CardWars.Server.Session;
using CardWars.Server.Vanilla.Network.Packet;
using CardWars.Vanilla.Shared.Network.Packet;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	public void OnLoad(ServerRegistry registry, List<ModContentResult> modContents)
	{
		var worldRegistry = new WorldRegistry();
		LoadWorldDefinitions(worldRegistry, modContents);

		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler(worldRegistry));
	}

	public void OnServerStart(Server server)
	{
		server.OnPlayerConnected += (session) =>
		{
			session.Connection.Send(new S2C_PlayerJoinedRequestPacket()
			{
				ServerGreetingMessage = "Hello this is from the server :)"
			});
		};

		server.OnPlayerInstanceChanged += (player, instance) =>
		{
			player.Connection.Send(new S2C_InstanceChangedPacket()
			{
				InstanceId = instance.InstanceId,
				InstanceType = instance.GetType().Name
			});
		};
	}

	private void LoadWorldDefinitions(WorldRegistry worldRegistry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["worlds"]:
					var worldDataTag = content.ReadAs<CompoundTag>();
					if (worldDataTag == null) continue;
					Logger.Info("Registered World: " + content.Id.ToString());
					worldRegistry.Templates.Register(content.Id, worldDataTag);
					break;
				case []:
					if (content.FilePath.GetFileNameWithoutExtension() == "config")
					{
						var configDataTag = content.ReadAs<CompoundTag>();
						if (configDataTag == null) continue;

						worldRegistry.DefaultWorld = ResourceId.Parse(configDataTag.GetString("default_world"));
						Logger.Info("Registered default_world as: " + worldRegistry.DefaultWorld);
					}
					break;
			}
		}
	}
}
