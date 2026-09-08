using CardWars.BattleEngine.Block;
using CardWars.BattleEngine.Vanilla.Block;
using CardWars.Client.scenes.core.game_session;
using CardWars.Client.scripts.core.packet;
using CardWars.Core.Network.Packet;
using CardWars.Vanilla.Shared;
using CardWars.Vanilla.Shared.Packet;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattleInstance : ClientInstance
{
	public Control? UI;
	public HandManager? HandManager;

	public override void _Ready()
	{
		UI = GetNode<Control>("UI");
		HandManager = GetNode<HandManager>("UI/HandManager");
		HandManager.BattleInstance = this;
	}

	public void CreateBattlefield()
	{
		var battlefield = registry?.GameObjects.Instantiate<Node3D>(SharedIds.Battlefield);
		if (battlefield == null) { return; }
		AddChild(battlefield);
	}

	public override void OnPacket(IPacket packet, PacketContextClient context)
	{
		if (packet is S2C_BattleBlockBatch battleBlockBatch) { ProcessBlockBatch(battleBlockBatch.Batch); }
	}

	public void ProcessBlockBatch(BlockBatch batch)
	{
		foreach (var block in batch.Blocks)
		{
			switch (block)
			{
				// IDK if this is the best way to handle this?
				case AttachBattlefieldToPlayerBlock attachBattlefieldToPlayer: break;
				case AttachCardToDeckBlock attachCardToDeck: break;
				case AttachCardToPlayerBlock attachCardToPlayer: break;
				case AttachCardToUnitSlotBlock attachCardToUnitSlot: break;
				case AttachDeckToPlayerBlock attachDeckToPlayer: break;
				case AttachUnitSlotToBattlefieldBlock attachUnitSlotToBattlefield: break;
				case DetachBattlefieldFromPlayerBlock detachBattlefieldFromPlayer: break;
				case DetachCardFromDeckBlock detachCardFromDeck: break;
				case DetachCardFromPlayerBlock detachCardFromPlayer: break;
				case DetachCardFromUnitSlotBlock detachCardFromUnitSlot: break;
				case DetachDeckFromPlayerBlock detachDeckFromPlayer: break;
				case DetachUnitSlotFromBattlefieldBlock detachUnitSlotFromBattlefield: break;
				case InstantiateBattlefieldBlock instantiateBattlefield: break;
				case InstantiateCardBlock instantiateCard: break;
				case InstantiateDeckBlock instantiateDeck: break;
				case InstantiatePlayerBlock instantiatePlayer: break;
				case InstantiateUnitSlotBlock instantiateUnitSlot: break;
				case ModifyUnitSlotPositionBlock modifyUnitSlotPosition: break;
				case SetCardDataBlock setCardDataBlock: break;
				case UpdateTurnStateBlock updateTurnState: break;
				default:
					break;
			}
		}

	}
}
