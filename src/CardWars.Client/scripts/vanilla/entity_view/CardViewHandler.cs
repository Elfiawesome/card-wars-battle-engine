using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Client.scripts.vanilla.registry;
using CardWars.Core.Logging;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scripts.vanilla.entity_view;

public class CardViewHandler : IEntityViewHandler<GenericCard>
{
	public void Sync(BattleInstance instance, IReadOnlyCollection<GenericCard> entities)
	{
		SyncBoard(instance, entities);
		SyncHand(instance, entities);
	}

	private static void SyncBoard(BattleInstance instance, IReadOnlyCollection<GenericCard> entities)
	{
		var boardCards = entities
			.Where(entity => entity.OwnerUnitSlotId is not null || entity.OwnerBattlefieldId is not null)
			.ToList();
		var boardIds = boardCards.Select(entity => entity.Id).ToHashSet();

		foreach (var id in instance.GetEntityNodeIds<Card3D>())
		{
			if (!boardIds.Contains(id)) instance.FreeEntityNode(id);
		}

		foreach (var card in boardCards)
		{
			var node = instance.GetOrCreateEntityNode<Card3D>(SharedIds.Entities.Card, card.Id);
			if (node is null) continue;

			if (card.OwnerUnitSlotId is { } slotId)
			{
				node.Rotation = Vector3.Zero;
				node.SetHitBox(false);
				AttachToSlot(instance, node, slotId);
			}
			else if (card.OwnerBattlefieldId is { } battlefieldId)
			{
				AttachToBattlefield(instance, node, battlefieldId);
				node.Rotation = new Vector3(0f, Mathf.Pi / 2f, 0f);
				node.SetHitBox(true);
				node.HookMouseEvents();
			}

			// TODO: Check for skins here
			// but for now let's use default
			var targetSkinId = SharedIds.UI.DefaultCardDisplay; // Set the skin here
			var display = instance.ClientRegistry?.UserInterface.Instantiate<CardDisplay>(targetSkinId);
			Log.Info(display);
			if (display is not null) node.SetCardDisplay(targetSkinId, display);
			node.SetCard(card);
		}
	}

	private static void AttachToBattlefield(BattleInstance instance, Card3D node, EntityId battlefieldId)
	{
		if (instance.GetEntityNode<BattlefieldNode>(battlefieldId) is not { } battlefield)
		{
			instance.AttachNodeToOwner(node, battlefieldId);
			return;
		}

		if (node.GetParent() == battlefield) return;

		node.GetParent()?.RemoveChild(node);
		battlefield.AddChild(node);
	}

	private static void AttachToSlot(BattleInstance instance, Card3D node, EntityId? slotId)
	{
		if (slotId is { } id && instance.GetEntityNode<UnitSlotNode>(id) is { } slot)
		{
			Node3D anchor = slot.CardAnchor ?? slot;
			if (node.GetParent() == anchor) return;

			node.GetParent()?.RemoveChild(node);
			anchor.AddChild(node);
			return;
		}

		instance.AttachNodeToOwner(node, slotId);
	}

	private static void SyncHand(BattleInstance instance, IReadOnlyCollection<GenericCard> entities)
	{
		var hand = instance.HandManagerNode;
		if (hand is null) return;

		var handIds = entities
			.Where(entity => entity.OwnerPlayerId == instance.MyPlayerId
				&& entity.OwnerUnitSlotId is null
				&& entity.OwnerBattlefieldId is null)
			.Select(entity => entity.Id)
			.ToHashSet();

		foreach (var id in hand.Cards.Keys.ToList())
		{
			if (!handIds.Contains(id)) hand.RemoveCard(id);
		}

		foreach (var id in handIds)
		{
			if (hand.GetCard(id) is null) hand.AddCard(id);
		}
	}
}
