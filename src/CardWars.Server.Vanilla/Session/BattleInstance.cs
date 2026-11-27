using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
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
	[DataTag] public GameStateSnapshot? Snapshot { get; set; }

	public BattleEngine.BattleEngine? Engine { get; private set; }
	public override bool RemoveWhenEmpty => true;

	public void InitializeEngine(BattleEngine.BattleEngine engine)
	{
		if (Engine is not null)
			throw new InvalidOperationException("Battle instance engine has already been initialized.");
		Engine = engine;

		if (Snapshot is { } snapshot)
			Engine.State.FromSnapshot(snapshot, Engine.Registry.DataTags);
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

		player.Connection.Send(new S2C_BattleSyncSnapshot() { GameStateSnapshot = Engine.State.ToSnapshot(Engine.Registry.DataTags) });

		var entityId = new EntityId(player.PlayerId);
		if (Engine.State.Get<Player>(entityId) is not null) return;

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
		Engine?.HandleInput(EntityId.None, new PlayerLeftRequestInput(new EntityId(player.PlayerId)));
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

		if (batch.TargetPlayerId is { IsNone: false } targetPlayerId)
		{
			foreach (var player in Players)
			{
				if (player.PlayerId != targetPlayerId.Value) continue;

				player.Connection.Send(packet);
				return;
			}
		}

		foreach (var player in Players)
			player.Connection.Send(packet);
	}
}
