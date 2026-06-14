using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Server.Vanilla.Packet;
using CardWars.Server.Vanilla.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	public void OnLoad(Server server, List<ModContentResult> modContents)
	{
		var worldRegistry = new WorldRegistry();
		RegisterPackets(server.Registry, worldRegistry);
		RegisterEvents(server, worldRegistry);
		LoadWorldDefinitions(worldRegistry, modContents);
		DataTagTypeRegistry.ScanAssembly(GetType().Assembly);
	}

	private void RegisterEvents(Server server, WorldRegistry worldRegistry)
	{
		server.OnPlayerConnected += (session) =>
		{
			session.Connection.Send(new S2C_PlayerJoinedRequestPacket()
			{
				ServerGreetingMessage = "Hello this is from the server :)"
			});
		};
	}

	private void RegisterPackets(ServerRegistry registry, WorldRegistry worldRegistry)
	{
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler(worldRegistry));
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