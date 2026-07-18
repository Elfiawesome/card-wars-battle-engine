using CardWars.Core.Data;
using CardWars.Core.Logging;
using CardWars.Core.Network.Transport;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Server.Session;
using CardWars.Server.Vanilla.Packet;
using CardWars.Server.Vanilla.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	public readonly string WorldKind = ResourceId.Vanilla("world").ToFlatString();
	public readonly string BattleKind = ResourceId.Vanilla("battle").ToFlatString();

	public void OnLoad(Server server, List<ModContentResult> modContents)
	{
		var worldRegistry = new WorldRegistry();
		LoadWorldDefinitions(worldRegistry, modContents);
		RegisterInstanceKinds(server, worldRegistry);
		RegisterPackets(server.Registry, worldRegistry, WorldKind);
		RegisterEvents(server, worldRegistry);
		DataTagTypeRegistry.ScanAssembly(GetType().Assembly);
	}

	private void RegisterInstanceKinds(Server server, WorldRegistry worldRegistry)
	{
		server.Registry.InstanceKinds.Register(WorldKind, new InstanceKindDescriptor(
			new WorldInstanceProvider(worldRegistry),
			InstancePolicy.SingletonByKey,
			InstancePersistence.Persistent));

		server.Registry.InstanceKinds.Register(BattleKind, new InstanceKindDescriptor(
			new BattleInstanceProvider(),
			InstancePolicy.Multi,
			InstancePersistence.Persistent));
	}

	private void RegisterPackets(ServerRegistry registry, WorldRegistry worldRegistry, string worldKind)
	{
		// Unauthenticated Packets
		registry.UnauthenticatedPacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler(worldRegistry, worldKind));

		// Normal Packets
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
	}

	private void RegisterEvents(Server server, WorldRegistry worldRegistry)
	{
		server.OnUnauthenticatedConnectionReceived += OnUnauthenticatedConnectionReceived;
	}

	private void OnUnauthenticatedConnectionReceived(IConnection connection)
	{
		connection.Send(new S2C_PlayerJoinedRequestPacket() { ServerGreetingMessage = "Hello! This is the server :)" });
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
