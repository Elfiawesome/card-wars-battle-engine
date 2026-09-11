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

	public void OnLoad(Server server, List<ModContentResult> modContents)
	{
		var worldRegistry = new WorldRegistry();
		RegisterPackets(server.Registry);
		RegisterEvents(server, worldRegistry);
		LoadWorldDefinitions(worldRegistry, modContents);

		server.Registry.ServerInstanceProviders.Register(SharedIds.WorldInstanceId, new WorldInstanceProvider(worldRegistry, server.Session, SharedIds.WorldInstanceId));
		server.Registry.ServerInstanceProviders.Register(SharedIds.BattleInstanceId, new BattleInstanceProvider(server.Session, server.SharedBattleEngineRegistry, SharedIds.BattleInstanceId));
	}

	private void RegisterPackets(ServerRegistry registry)
	{
		registry.UnauthenticatedPacketHandlers.Register(new C2S_PlayerJoinedRequestResponsePacketHandler());
		registry.PacketHandlers.Register(new C2S_CustomModPacketHandler());
		registry.PacketHandlers.Register(new C2S_DEBUG_WarpRequestPacketHandler());
		registry.PacketHandlers.Register(new C2S_DEBUG_EnterBattlePacketHandler(this));
	}

	public Guid GetOrCreateCurrentBattle(Server server)
	{
		// TODO: Remove later. This is only testing battles
		if (_currentBattleInstanceId is { } existing)
			return existing;

		// Battles are not persisted yet; generate a fresh id each session.
		var battleId = Guid.NewGuid().ToString();
		_currentBattleInstanceId = server.CreateInstance(SharedIds.BattleInstanceId, battleId).InstanceId;
		return _currentBattleInstanceId.Value;
	}

	private void RegisterEvents(Server server, WorldRegistry worldRegistry)
	{
		server.OnUnauthenticatedConnectionReceived += OnUnauthenticatedConnectionReceived;
		server.OnAddPlayer += player => OnPlayerJoined(server, worldRegistry, player);
		server.OnRemovePlayer += player => OnPlayerLeft(server, player);
		server.OnPlayerPreEnterInstance += OnPlayerPreEnterInstance;
		server.OnPlayerPostEnterInstance += OnPlayerPostEnterInstance;
		server.OnPlayerLeaveInstance += OnPlayerLeaveInstance;
	}

	private void OnUnauthenticatedConnectionReceived(IConnection connection)
	{
		connection.Send(new S2C_PlayerJoinedRequestPacket() { ServerGreetingMessage = "Hello! This is the server :)" });
	}

	private void OnPlayerJoined(Server server, WorldRegistry worldRegistry, PlayerSession player)
	{
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
			Log.Warn("No default world configured; player was not placed into a world.");
			return;
		}

		server.EnterInstance(player, SharedIds.WorldInstanceId, worldRegistry.DefaultWorld.ToString());
	}

	private void OnPlayerLeft(Server server, PlayerSession player)
	{
		// Core teardown (leaving instance + saving player) is handled by Server.RemovePlayer.
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

	private void LoadWorldDefinitions(WorldRegistry worldRegistry, List<ModContentResult> modContents)
	{
		foreach (var content in modContents)
		{
			switch (content.Category)
			{
				case ["worlds"]:
					var worldDataTag = content.ReadAs<CompoundTag>();
					if (worldDataTag == null) continue;
					Log.Info("Registered World: " + content.Id.ToString());
					worldRegistry.Templates.Register(content.Id, worldDataTag);
					break;
				case []:
					if (content.FilePath.GetFileNameWithoutExtension() == "config")
					{
						var configDataTag = content.ReadAs<CompoundTag>();
						if (configDataTag == null) continue;

						worldRegistry.DefaultWorld = ResourceId.Parse(configDataTag.GetString("default_world"));
						Log.Info("Registered default_world as: " + worldRegistry.DefaultWorld);
					}
					break;
			}
		}
	}
}