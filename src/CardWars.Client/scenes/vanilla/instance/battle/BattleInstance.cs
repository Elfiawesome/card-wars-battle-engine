using CardWars.BattleEngine;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattleInstance : ClientInstance
{
	public BattleRegistry? BattleRegistry => ClientRegistry?.GetExtension<BattleRegistry>();

	public Control? UINode;
	public HandManager? HandManagerNode;
	public Node3D? PlayspaceNode;
	public GameState State = new();

	public override void _Ready()
	{
		UINode = GetNode<Control>("UI");
		HandManagerNode = GetNode<HandManager>("UI/HandManager");
		PlayspaceNode = GetNode<Node3D>("Playspace");
		HandManagerNode.BattleInstance = this;
	}

	public override void OnPacket(IPacket packet, PacketContextClient context)
	{
		if (packet is S2C_BattleBlockBatch battleBlockBatch) { ProcessBlockBatch(battleBlockBatch.Batch); }
	}

	public void ProcessBlockBatch(BlockBatch batch)
	{
		var battleRegsitry = ClientRegistry?.GetExtension<BattleRegistry>();
		foreach (var block in batch.Blocks)
		{
			// TODO: RUN

		}
	}
}