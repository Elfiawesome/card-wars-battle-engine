using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Network.Packet;
using CardWars.Core.Registry;
using CardWars.Server.Packet;
using CardWars.Server.Session;
using CardWars.Vanilla.Shared.Packet;

namespace CardWars.Server.Vanilla.Session;

[DataTagType()]
public class BattleInstance : ServerInstance
{
	[DataTag] public override Guid InstanceId { get; set; }
	[DataTag] public override ResourceId InstanceProviderId { get; set; }

	public BattleEngine.BattleEngine? Engine { get; set; }

	public override void Ready()
	{
		base.Ready();
		// if (Engine != null) Engine.State.Layout = new(ResourceId.Vanilla("line"), new());
		SetupTestBattle();
	}

	private void SetupTestBattle()
	{
		void addTestPlayer(int team) => Engine?.HandleInput(EntityId.None, new PlayerJoinedRequestInput(new EntityId(Guid.NewGuid()), team));
		// 1v1
		// addTestPlayer(2);

		// 2v2
		// addTestPlayer(1);
		// addTestPlayer(2); addTestPlayer(2);

		// 3v2v1
		// addTestPlayer(2); addTestPlayer(2);
		// addTestPlayer(2); addTestPlayer(2); addTestPlayer(3);

		// FFA
		// addTestPlayer(2); addTestPlayer(3); addTestPlayer(4); addTestPlayer(5);

		// Absurd Testing
		// for (var i = 0; i < 6; i++) { addTestPlayer(2); }
	}

	public override void AddPlayer(PlayerSession player)
	{
		base.AddPlayer(player);

		if (Engine == null) return;
		// Send Player current State
		player.Connection.Send(new S2C_BattleSyncSnapshot() { GameStateSnapshot = Engine.State.ToSnapshot() });

		// Bind the battle player identity to the session identity so they map 1:1.
		var entityId = new EntityId(player.PlayerId);
		Engine?.HandleInput(EntityId.None, new PlayerJoinedRequestInput(entityId));
	}

	public override void RemovePlayer(PlayerSession player)
	{
		base.RemovePlayer(player);
		// TODO: player leave battle input / end battle when empty.
	}

	public override void HandlePacket(PacketContextServer context, IPacket packet)
	{
		switch (packet)
		{
			case C2S_BattleInput battleInput:
				var entityId = new EntityId(context.PlayerSession.PlayerId);
				Engine?.HandleInput(entityId, battleInput.Input);
				break;
		}
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
