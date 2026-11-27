using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class BattleInstance : ServerInstance
{
	[DataTag] public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	public BattleEngine.BattleEngine? Engine { get; private set; }

	public void InitializeEngine(BattleEngine.BattleEngine engine)
	{
		if (Engine is not null)
			throw new InvalidOperationException("Battle instance engine has already been initialized.");
		Engine = engine;
	}

	public override bool CanAddPlayer(PlayerSession player, out string? reason)
		=> CreateBattleSetup(player, out reason) is not null;

	public override void AddPlayer(PlayerSession player)
	{
		var setup = CreateBattleSetup(player, out var reason);
		if (setup is null)
		{
			Engine?.Registry.Logger.Error($"Cannot add player [{player.PlayerId}] to battle: {reason}");
			return;
		}

		base.AddPlayer(player);

		if (Engine is null) return;
		// Send Player current State
		player.Connection.Send(new S2C_BattleSyncSnapshot() { GameStateSnapshot = Engine.State.ToSnapshot() });

		// Bind the battle player identity to the session identity so they map 1:1.
		var entityId = new EntityId(player.PlayerId);
		Engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(entityId, Players.Count % 2, setup));
	}

	private static PlayerBattleSetup? CreateBattleSetup(PlayerSession player, out string? reason)
	{
		var profile = player.GetSetComponent<ProfilePlayerDataComponent>(SharedIds.PlayerData.Profile).Profile;
		if (profile is null)
		{
			reason = "player has no profile";
			return null;
		}

		if (profile.CurrentDeck.Count == 0)
		{
			reason = "player profile has no deck";
			return null;
		}

		if (profile.Heroes.Count == 0)
		{
			reason = "player profile has no heroes";
			return null;
		}

		reason = null;
		return new PlayerBattleSetup
		{
			Deck = [.. profile.CurrentDeck],
			Heroes = [.. profile.Heroes]
		};
	}

	public override void RemovePlayer(PlayerSession player)
	{
		base.RemovePlayer(player);
		// TODO: player leave battle input / end battle when empty.
	}

	public override bool HandlePacket(PacketContextServer context, IPacket packet)
	{
		switch (packet)
		{
			case C2S_BattleInput battleInput:
				var entityId = new EntityId(context.PlayerSession.PlayerId);
				Engine?.HandleInput(entityId, battleInput.Input);
				return true;
		}

		return false;
	}

	public override void Tick(float deltaTime) { }

	public void BroadcastBatch(BlockBatch batch)
	{
		var packet = new S2C_BattleBlockBatch { Batch = batch };
		foreach (var player in Players)
			// TODO: TargetPlayerId not accounted for!
			player.Connection.Send(packet);
	}
}
