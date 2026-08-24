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
	private static readonly ResourceId WorldProviderKey = ResourceId.Vanilla("world");
	private static readonly ResourceId BattleProviderKey = ResourceId.Vanilla("battle");

	public void OnLoad(Server server, List<ModContentResult> modContents)
	{
		var worldRegistry = new WorldRegistry();
		RegisterPackets(server.Registry);
		RegisterEvents(server, worldRegistry);
		LoadWorldDefinitions(worldRegistry, modContents);

		server.Registry.ServerInstanceProviders.Register(WorldProviderKey, new WorldInstanceProvider(worldRegistry, server.Session));
		server.Registry.ServerInstanceProviders.Register(BattleProviderKey, new BattleInstanceProvider());
	}

	private void RegisterPackets(ServerRegistry registry)
	{
		registry.UnauthenticatedPacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler());
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
	}

	private void RegisterEvents(Server server, WorldRegistry worldRegistry)
	{
		server.OnUnauthenticatedConnectionReceived += OnUnauthenticatedConnectionReceived;
		server.OnAddPlayer += player => OnPlayerJoined(server, worldRegistry, player);
		server.OnRemovePlayer += player => OnPlayerLeft(server, player);
	}

	private void OnUnauthenticatedConnectionReceived(IConnection connection)
	{
		connection.Send(new S2C_PlayerJoinedRequestPacket() { ServerGreetingMessage = "Hello! This is the server :)" });
	}

	private void OnPlayerJoined(Server server, WorldRegistry worldRegistry, PlayerSession player)
	{
		if (string.IsNullOrEmpty(player.CurrentInstanceSaveName))
			player.CurrentInstanceSaveName = worldRegistry.DefaultWorld.ToFlatString();

		var instance = server.LoadInstance(player.CurrentInstanceSaveName);

		if (instance == null)
		{
			var instanceIdStr = worldRegistry.DefaultWorld.ToString();
			instance = server.CreateInstance(WorldProviderKey, instanceIdStr);
			if (instance == null)
			{
				Logger.Error($"Failed to create instance for player [{player.Username}]");
				return;
			}
			server.SaveInstance(WorldProviderKey, instance, player.CurrentInstanceSaveName, instanceIdStr);
		}

		player.CurrentInstance = instance;
		instance.AddPlayer(player);
		server.Session.SavePlayer(player.PlayerId, DataTagMapper.ToTag(player, false));

		Logger.Info($"[{player.Username}] joined instance '{instance.InstanceId}' ({player.CurrentInstanceSaveName})");
	}

	private void OnPlayerLeft(Server server, PlayerSession player)
	{
		var instance = player.CurrentInstance;
		if (instance != null)
		{
			instance.RemovePlayer(player);
			player.CurrentInstance = null;

			if (!string.IsNullOrEmpty(player.CurrentInstanceSaveName))
				server.SaveInstance(player.CurrentInstanceSaveName, instance);
		}

		server.Session.SavePlayer(player.PlayerId, DataTagMapper.ToTag(player, false));
		Logger.Info($"[{player.Username}] left instance and data saved.");
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