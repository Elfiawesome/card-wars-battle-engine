using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Logging;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared;
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
	private readonly Dictionary<EntityId, Node3D> _entityNodes = [];


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
		foreach (var block in batch.Blocks)
		{
			BattleEngineRegistry?.BlockHandlers.Execute(State, block);
		}
		SyncState();
		Log.Info(State);
	}

	public void SyncState()
	{
		var currentIds = State.All.Select(e => e.Id).ToHashSet();
		var toRemove = _entityNodes.Keys.Except(currentIds).ToList();
		foreach (var id in toRemove)
		{
			var node = _entityNodes[id];
			node.QueueFree(); // DONT DO THIS!! Set this to make it run animations first then get rid of it
			_entityNodes.Remove(id);
		}


		if (PlayspaceNode == null) return;
		foreach (var entity in State.All)
		{
			switch (entity)
			{
				case Battlefield battlefield:
					{
						var hasNode = PlayspaceNode.HasNode(battlefield.Id.ToString());
						if (hasNode)
						{
							// Update
							var node = PlayspaceNode.GetNode(battlefield.Id.ToString());
							if (node == null) { return; }
						}
						else
						{
							// Create
							var battlefieldNode = ClientRegistry?.GetExtension<BattleRegistry>()?.EntityScene.Instantiate<Node3D>(SharedIds.Battlefield);
							if (battlefieldNode == null) { return; }

							battlefieldNode.Name = battlefield.Id.ToString();
							_entityNodes.Add(battlefield.Id, battlefieldNode);
							PlayspaceNode.AddChild(battlefieldNode);
						}
					}
					break;
				case GenericCard card:

					break;
				case Deck deck:
					break;
				case Player player:
					break;
				case UnitSlot unitSlot:
					{
						var hasNode = PlayspaceNode.HasNode(unitSlot.Id.ToString());
						if (hasNode)
						{
							// Update
							var node = PlayspaceNode.GetNode(unitSlot.Id.ToString());
							if (node == null) { return; }
						}
						else
						{
							if (unitSlot.OwnerBattlefieldId == null) { return; }

							// Create
							var unitSlotNode = ClientRegistry?.GetExtension<BattleRegistry>()?.EntityScene.Instantiate<Node3D>(SharedIds.UnitSlot);
							if (unitSlotNode == null) { return; }

							unitSlotNode.Name = entity.Id.ToString();
							_entityNodes.Add(unitSlot.Id, unitSlotNode);
							if (_entityNodes.TryGetValue((EntityId)unitSlot.OwnerBattlefieldId, out var battlefield))
							{
								battlefield.AddChild(unitSlotNode);
							}
						}
					}
					break;
			}
		}
	}
}