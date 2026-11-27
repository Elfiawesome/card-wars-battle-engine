using CardWars.BattleEngine.registry;
using CardWars.Core.Data;
using CardWars.Core.Data.Tags;
using CardWars.Core.Logging;
using CardWars.Core.Network.Transport;
using CardWars.Core.Registry;
using CardWars.ModLoader;
using CardWars.Server.Session;
using CardWars.Server.Vanilla.Packet;
using CardWars.Server.Vanilla.Session;
using CardWars.Vanilla.Shared;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla;

public class VanillaMod : IServerMod
{
	private Guid? _currentBattleInstanceId;

	public void OnLoad(Server server, IReadOnlyList<ModContentResult> modContents)
	{
		// Local registries (relative to this mod only)
		var worldRegistry = new WorldRegistry();
		var profileRegistry = new PlayerProfileRegistry();
		RegisterPackets(server.Registry, profileRegistry);
		RegisterEvents(server, worldRegistry, profileRegistry);
		LoadWorldDefinitions(worldRegistry, profileRegistry, modContents, server.Session.DataTags, server.Logger);
		LoadPlayerProfiles(profileRegistry, modContents, server.Session.DataTags, server.Logger); // Get the default starter profile and register it in profileRegistry

		// Get extension registries (this ones arent local :0)
		var battleEngineRegistry = server.Registry.GetExtension<BattleEngineRegistry>()
			?? throw new InvalidOperationException("Battle engine registry extension is required.");

		// Register instances
		server.Registry.ServerInstanceProviders.Register(SharedIds.Instances.World, new WorldInstanceProvider(worldRegistry, server.Session, SharedIds.Instances.World, server.Logger));
		server.Registry.ServerInstanceProviders.Register(SharedIds.Instances.Battle, new BattleInstanceProvider(server.Session, battleEngineRegistry, SharedIds.Instances.Battle));
	}

	private void RegisterPackets(ServerRegistry registry, PlayerProfileRegistry profileRegistry)
	{
		registry.UnauthenticatedPacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler(profileRegistry));
		registry.PacketHandlers.Register(new C2S_DEBUG_WarpRequestPacketHandler());
		registry.PacketHandlers.Register(new C2S_DEBUG_EnterBattlePacketHandler(this));
	}

	public Guid GetOrCreateCurrentBattle(Server server)
	{
		// TODO: Remove later. This is only testing battles
		if (_currentBattleInstanceId is { } existing)
			return existing;

		// Debug battle entry creates a fresh battle per server process.
		var battleId = Guid.NewGuid().ToString();
		_currentBattleInstanceId = server.CreateInstance(SharedIds.Instances.Battle, battleId).InstanceId;
		return _currentBattleInstanceId.Value;
	}

	private void RegisterEvents(Server server, WorldRegistry worldRegistry, PlayerProfileRegistry profileRegistry)
	{
		server.OnUnauthenticatedConnectionReceived += OnUnauthenticatedConnectionReceived;
		server.OnAddPlayer += player => OnPlayerJoined(server, worldRegistry, profileRegistry, player);
		server.OnRemovePlayer += player => OnPlayerLeft(server, player);
		server.OnPlayerPreEnterInstance += OnPlayerPreEnterInstance;
		server.OnPlayerPostEnterInstance += OnPlayerPostEnterInstance;
		server.OnPlayerLeaveInstance += OnPlayerLeaveInstance;
		server.OnPlayerEnterInstanceRejected += OnPlayerEnterInstanceRejected;
	}

	private void OnUnauthenticatedConnectionReceived(IConnection connection)
	{
		connection.Send(new S2C_PlayerJoinedRequestPacket() { ServerGreetingMessage = "Hello! This is the server :)" });
	}

	private void OnPlayerJoined(Server server, WorldRegistry worldRegistry, PlayerProfileRegistry profileRegistry, PlayerSession player)
	{
		// Get Player Profile and/or Set default profile
		var profileData = player.GetSetComponent<ProfilePlayerDataComponent>(SharedIds.PlayerData.Profile);
		if (profileData.Profile is null && profileRegistry.Starter is { } starter)
			profileData.Profile = starter.Copy();

		// Rejoin the instance the player last left (world or battle) if known.
		var providerId = player.CurrentInstanceProvider;
		var saveName = player.CurrentInstanceSaveName;
		if (!providerId.IsEmpty && !string.IsNullOrEmpty(saveName))
		{
			server.EnterInstance(player, providerId, saveName);
			return;
		}

		if (worldRegistry.DefaultWorld.IsEmpty)
		{
			server.Logger.Warn("No default world configured; player was not placed into a world.");
			return;
		}

		server.EnterInstance(player, SharedIds.Instances.World, worldRegistry.DefaultWorld.ToString());
	}

	private void OnPlayerLeft(Server server, PlayerSession player)
	{
		// Core teardown (leaving instance + saving player) is handled by Server.RemovePlayer.
	}

	private void OnPlayerEnterInstanceRejected(Server server, IServerInstance instance, PlayerSession player, string reason)
	{
		player.Connection.Send(new S2C_EnterInstanceRejectedPacket
		{
			PlayerId = player.PlayerId,
			ProviderId = instance.InstanceProviderId,
			Reason = reason
		});
	}

	private void OnPlayerPreEnterInstance(Server server, IServerInstance instance, PlayerSession player)
	{
		var enterPacket = new S2C_EnterInstancePacket
		{
			ProviderId = instance.InstanceProviderId,
			PlayerId = player.PlayerId
		};
		player.Connection.Send(enterPacket);
	}

	private void OnPlayerPostEnterInstance(Server server, IServerInstance instance, PlayerSession player)
	{
		switch (instance)
		{
			case WorldInstance world:
				world.BroadcastSnapshot();
				break;
			case BattleInstance battle:
				break;
		}
	}

	private void OnPlayerLeaveInstance(Server server, IServerInstance instance, PlayerSession player)
	{
		if (instance is WorldInstance worldInstance)
		{
			worldInstance.BroadcastSnapshot();
		}
		else
		{
			foreach (var playerSession in instance.Players)
			{
				playerSession.Connection.Send(
					new S2C_LeaveInstancePacket() { PlayerId = player.PlayerId }
				);
			}
		}
	}

	private void LoadPlayerProfiles(PlayerProfileRegistry profileRegistry, IReadOnlyList<ModContentResult> modContents, DataTagContext dataTags, Logger logger)
	{
		foreach (var content in modContents)
		{
			if (content.Category is not ["profiles"]) continue;

			var profile = content.ReadAsObject<PlayerProfile>(dataTags);
			profileRegistry.Starter = profile;
			logger.Info($"Registered starter profile: {content.Id}");
		}
	}

	private void LoadWorldDefinitions(WorldRegistry worldRegistry, PlayerProfileRegistry profileRegistry, IReadOnlyList<ModContentResult> modContents, DataTagContext dataTags, Logger logger)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["worlds"]:
					var worldDataTag = content.ReadAs<CompoundTag>(dataTags);
					if (worldDataTag is null) continue;
					logger.Info("Registered World: " + content.Id.ToString());
					worldRegistry.Templates.Register(content.Id, worldDataTag);
					break;
				case []:
					if (content.FilePath.GetFileNameWithoutExtension() == "config")
					{
						var configDataTag = content.ReadAs<CompoundTag>(dataTags);
						if (configDataTag is null) continue;

						worldRegistry.DefaultWorld = ResourceId.Parse(configDataTag.GetString("default_world"));
						logger.Info("Registered default_world as: " + worldRegistry.DefaultWorld);

						var authority = configDataTag.GetString("profile_authority", "server");
						profileRegistry.Authority = authority.Equals("client", StringComparison.OrdinalIgnoreCase)
							? ProfileAuthority.Client
							: ProfileAuthority.Server;
						logger.Info($"Profile authority: {profileRegistry.Authority}");
					}
					break;
			}
		}
	}
}