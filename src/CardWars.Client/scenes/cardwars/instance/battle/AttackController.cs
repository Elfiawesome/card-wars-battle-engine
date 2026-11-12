using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Attacks;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Client.scenes.cardwars.instance.battle.entities;
using CardWars.Client.scenes.vanilla.instance.battle;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.cardwars.instance.battle;

public partial class AttackController : Node
{
	public BattleInstance? BattleInstance;

	private readonly HashSet<EntityId> _selected = [];
	private readonly Dictionary<EntityId, LineArrow> _arrows = [];
	private readonly Dictionary<EntityId, HashSet<EntityId>> _targets = [];

	public void Setup(BattleInstance battleInstance)
	{
		BattleInstance = battleInstance;
		BattleInstance.OnHovered3DTargetClicked += HandleClick;
	}

	public override void _Process(double delta)
	{
		if (BattleInstance is null) return;

		if (!CanAct())
		{
			Clear();
			return;
		}

		if (BattleInstance.HandManagerNode is not CanvasItem arrowParent) return;
		if (BattleInstance.CameraNode is not { } camera) return;

		foreach (var attackerId in _selected)
		{
			if (BattleInstance.GetEntityNode<UnitSlotNode>(attackerId) is not { } slot) continue;
			if (!_arrows.TryGetValue(attackerId, out var arrow)) continue;
			if (!_targets.TryGetValue(attackerId, out var targets)) continue;

			var source = slot.CardAnchor?.GlobalPosition ?? slot.GlobalPosition;
			arrow.SetScreenOrigin(arrowParent, camera.UnprojectPosition(source));

			if (BattleInstance.Hovered3DTarget is { } hovered && targets.Contains(hovered.EntityId))
				arrow.SnapTo(camera, hovered);
			else
				arrow.FollowMouse();
		}
	}

	public override void _ExitTree()
	{
		if (BattleInstance is null) return;

		Clear();
		BattleInstance.OnHovered3DTargetClicked -= HandleClick;
		BattleInstance = null;
	}

	public void HandleClick(EntityNode3D? clicked)
	{
		if (BattleInstance is null) return;
		if (BattleInstance.HandManagerNode?.SelectedCard is not null) return;
		if (!CanAct()) return;

		if (clicked is Card3D heroNode
			&& BattleInstance.State.Get<GenericCard>(heroNode.EntityId) is { IsHero: true })
		{
			AttackHeroTarget(heroNode);
			return;
		}

		if (clicked is not UnitSlotNode slot) return;
		if (slot.GetData<UnitSlot>() is not { } unitSlot) return;

		var ownerPlayerId = AttackTargetResolver.GetOwnerPlayerId(BattleInstance.State, unitSlot);
		if (ownerPlayerId == BattleInstance.MyPlayerId)
		{
			if (slot.GetData<GenericCard>(unitSlot.HoldingCardId) is not { } card) return;
			if (BattleInstance.ActionValidator is not { } actions) return;
			if (!actions.TryResolveAttack(BattleInstance.State, BattleInstance.MyPlayerId, card, out var attack, out _)) return;

			ToggleAttacker(slot, card, attack);
			return;
		}

		if (_selected.Count == 0) return;
		AttackTarget(unitSlot);
	}

	public void OnStateSynced()
	{
		if (BattleInstance is null) return;

		PruneSelection();
		RefreshHighlights();
	}

	private void ToggleAttacker(UnitSlotNode slot, GenericCard card, AttackProfile attack)
	{
		if (_selected.Contains(slot.EntityId))
		{
			RemoveAttacker(slot.EntityId);
			RefreshHighlights();
			return;
		}

		_selected.Add(slot.EntityId);
		slot.SetSelected(true);
		RefreshTargets(slot.EntityId, card, attack);
		CreateArrow(slot.EntityId);
		RefreshHighlights();
	}

	private void AttackTarget(UnitSlot targetSlot)
	{
		if (BattleInstance is null || BattleInstance.ActionValidator is not { } actions) return;

		var validAttackers = new List<EntityId>();

		foreach (var attackerId in _selected)
		{
			if (BattleInstance.GetEntityNode<UnitSlotNode>(attackerId) is not { } attackerNode) continue;
			if (attackerNode.GetData<UnitSlot>() is not { } attackerSlot) continue;
			if (attackerSlot.HoldingCardId is not { } attackerCardId) continue;
			if (BattleInstance.State.Get<GenericCard>(attackerCardId) is not { } attackerCard) continue;

			if (!actions.TryResolveAttack(BattleInstance.State, BattleInstance.MyPlayerId, attackerCard, out var attack, out _)) continue;
			if (!actions.CanTargetUnit(BattleInstance.State, attackerCard, attack, targetSlot.Id, out _)) continue;

			validAttackers.Add(attackerId);
		}

		if (validAttackers.Count == 0) return;

		BattleInstance.SendInput(new AttackRequestInput(validAttackers, targetSlot.Id));

		foreach (var attackerId in validAttackers)
			RemoveAttacker(attackerId);

		RefreshHighlights();
	}

	private void AttackHeroTarget(Card3D heroNode)
	{
		if (BattleInstance is null || BattleInstance.ActionValidator is not { } actions) return;
		if (BattleInstance.State.Get<GenericCard>(heroNode.EntityId) is not { IsHero: true } hero) return;

		var validAttackers = new List<EntityId>();

		foreach (var attackerId in _selected)
		{
			if (BattleInstance.GetEntityNode<UnitSlotNode>(attackerId) is not { } attackerNode) continue;
			if (attackerNode.GetData<UnitSlot>() is not { } attackerSlot) continue;
			if (attackerSlot.HoldingCardId is not { } attackerCardId) continue;
			if (BattleInstance.State.Get<GenericCard>(attackerCardId) is not { } attackerCard) continue;

			if (!actions.TryResolveAttack(BattleInstance.State, BattleInstance.MyPlayerId, attackerCard, out _, out _)) continue;
			if (!actions.CanTargetHero(BattleInstance.State, attackerCard, hero, out _)) continue;

			validAttackers.Add(attackerId);
		}

		if (validAttackers.Count == 0) return;

		BattleInstance.SendInput(new AttackHeroRequestInput(validAttackers, hero.Id));

		foreach (var attackerId in validAttackers)
			RemoveAttacker(attackerId);

		RefreshHighlights();
	}

	private void PruneSelection()
	{
		if (BattleInstance is null) return;
		if (_selected.Count == 0) return;
		if (BattleInstance.ActionValidator is not { } actions) return;

		foreach (var attackerId in _selected.ToList())
		{
			if (BattleInstance.GetEntityNode<UnitSlotNode>(attackerId) is not { } slot)
			{
				RemoveAttacker(attackerId);
				continue;
			}

			if (slot.GetData<UnitSlot>() is not { } unitSlot)
			{
				RemoveAttacker(attackerId);
				continue;
			}

			if (unitSlot.HoldingCardId is not { } cardId || BattleInstance.State.Get<GenericCard>(cardId) is not { } card)
			{
				RemoveAttacker(attackerId);
				continue;
			}

			if (!actions.TryResolveAttack(BattleInstance.State, BattleInstance.MyPlayerId, card, out var attack, out _))
			{
				RemoveAttacker(attackerId);
				continue;
			}

			RefreshTargets(attackerId, card, attack);
		}
	}

	private void RefreshHighlights()
	{
		if (BattleInstance is null) return;

		if (_selected.Count == 0)
		{
			BattleInstance.TargetHighlighter.Clear(this);
			return;
		}

		var targets = _targets.Values.SelectMany(target => target).ToHashSet();
		BattleInstance.TargetHighlighter.Set(this, targets);
	}

	private void RefreshTargets(EntityId attackerId, GenericCard card, AttackProfile attack)
	{
		if (BattleInstance?.ActionValidator is not { } actions) return;

		_targets[attackerId] =
		[
			.. actions.GetValidUnitTargets(BattleInstance.State, card, attack),
			.. actions.GetValidHeroTargets(BattleInstance.State, card)
		];
	}

	private void CreateArrow(EntityId attackerId)
	{
		if (BattleInstance?.HandManagerNode is not { } parent) return;
		if (BattleInstance.Registry.Client?.UserInterface.Instantiate<LineArrow>(SharedIds.UI.LineArrow) is not { } arrow) return;

		parent.AddChild(arrow);
		_arrows[attackerId] = arrow;
	}

	private void RemoveAttacker(EntityId attackerId)
	{
		_selected.Remove(attackerId);
		_targets.Remove(attackerId);

		if (BattleInstance?.GetEntityNode<UnitSlotNode>(attackerId) is { } slot)
			slot.SetSelected(false);

		if (_arrows.Remove(attackerId, out var arrow))
			arrow.QueueFree();
	}

	private void Clear()
	{
		foreach (var attackerId in _selected.ToList())
			RemoveAttacker(attackerId);
		BattleInstance?.TargetHighlighter.Clear(this);
	}

	private bool CanAct()
		=> BattleInstance is not null
			&& BattleInstance.State.Turn.Phase == TurnPhase.Attacking
			&& BattleInstance.State.Turn.AllowedPlayerInputs.Contains(BattleInstance.MyPlayerId);
}
