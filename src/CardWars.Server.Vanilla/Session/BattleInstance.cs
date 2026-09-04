using CardWars.BattleEngine;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Input;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Data;
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

	public BattleEngine.BattleEngine? Engine { get; set; }

	public override void AddPlayer(PlayerSession player)
	{
		base.AddPlayer(player);

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
