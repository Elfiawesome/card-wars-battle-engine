using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Logging;
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

	// TODO: Replace this temporary battle setup with proper battle-entry rejection/profile validation once deck selection exists.
	private static readonly PlayerBattleSetup TemporaryBattleSetup = new()
	{
		Deck = [ResourceId.Vanilla("cards/units/plantvszombies/cob_cannon")],
		Heroes = [ResourceId.Vanilla("cards/heroes/captain")]
	};

	public BattleEngine.BattleEngine? Engine { get; set; }

	public override void AddPlayer(PlayerSession player)
	{
		var profile = player.GetSetComponent<ProfilePlayerDataComponent>(SharedIds.PlayerData.Profile).Profile;
		PlayerBattleSetup setup;

		if (profile is null)
		{
			Log.Warn($"Player [{player.PlayerId}] has no usable profile; using temporary battle setup");
			setup = TemporaryBattleSetup;
		}
		else
		{
			setup = new PlayerBattleSetup
			{
				Deck = [.. profile.CurrentDeck],
				Heroes = [.. profile.Heroes]
			};
		}

		base.AddPlayer(player);

		if (Engine is null) return;
		// Send Player current State
		player.Connection.Send(new S2C_BattleSyncSnapshot() { GameStateSnapshot = Engine.State.ToSnapshot() });

		// Bind the battle player identity to the session identity so they map 1:1.
		var entityId = new EntityId(player.PlayerId);
		Engine.HandleInput(EntityId.None, new PlayerJoinedRequestInput(entityId, Players.Count % 2, setup));
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
