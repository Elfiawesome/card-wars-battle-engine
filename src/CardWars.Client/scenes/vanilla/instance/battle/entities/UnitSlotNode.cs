using System;
using System.Collections.Generic;
using System.Linq;
using CardWars.BattleEngine.State;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.BattleEngine.Vanilla.Features;
using CardWars.Core.Logging;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class UnitSlotNode : EntityNode3D
{
	public const float CellWidth = 0.9f;
	public const float CellDepth = 1.1f;

	public Vector3 TargetPosition { get; private set; } = Vector3.Zero;
	public Node3D? CardAnchor { get; private set; }
	public bool IsSelected { get; private set; } = false;

	public override void _Ready()
	{
		base._Ready();

		CardAnchor = GetNodeOrNull<Node3D>("CardAnchor");

		// This code
		var collisionBox = GetNode<Area3D>("Area3D");
		collisionBox.InputEvent += OnCollisionInputEvent;
		collisionBox.MouseEntered += () => { OnMouseEntered?.Invoke(); };
		collisionBox.MouseExited += () => { OnMouseExited?.Invoke(); };
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (CardAnchor is not null)
		{
			GetNode<MeshInstance3D>("MeshInstance3D").Visible = !(CardAnchor.GetChildCount() > 0);

			if (BattleInstance?.CameraNode is not { } camera || !IsInstanceValid(camera)) return;

			if (CardAnchor.GetParent() is Node3D parent)
			{
				var flipped = parent.GlobalBasis.Z.Dot(camera.GlobalBasis.Y) > 0f;
				CardAnchor.Rotation = new Vector3(CardAnchor.Rotation.X, flipped ? Mathf.Pi : 0f, CardAnchor.Rotation.Z);
			}

			if (IsSelected)
			{
				var sin = (float)Math.Sin(Time.GetTicksMsec() / 1000.0f * 5.0f) * 0.05f;
				CardAnchor.Scale = new Vector3(1 - sin, 1, 1 - sin);
			}
			else
			{
				CardAnchor.Scale = Vector3.One;
			}
		}
	}


	private void OnCollisionInputEvent(Node cameraNode, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIndex)
	{
		if (@event is not InputEventMouseButton inputEventMouseButton
			|| !inputEventMouseButton.Pressed
			|| inputEventMouseButton.ButtonIndex != MouseButton.Left) { return; }

		// Generic clicking...
		OnMouseClicked?.Invoke();

		// Huh probably should be cleaner or data driven lol
		if (BattleInstance is { } battleInstance)
		{
			if (
				// Check UnitSlot exists
				TryGetData<UnitSlot>(out var nullUnitSlot)
				&& nullUnitSlot is { } unitSlot
				&& unitSlot?.OwnerBattlefieldId is { } battlefieldId
				// Check Battlefield exists
				&& TryGetData<Battlefield>(out var nullBattlefield, battlefieldId)
				&& nullBattlefield is { } battlefield
				&& battlefield?.OwnerPlayerId is { } ownerPlayerId
				// Check Player exists
				&& TryGetData<Player>(out var nullPlayer, battlefield.OwnerPlayerId)
				&& nullPlayer is { } player
				// Check if it's my allowed turn
				&& battleInstance.State.Turn.AllowedPlayerInputs.Contains(battleInstance.MyPlayerId)
			)
			{
				if (battleInstance.MyPlayerId == ownerPlayerId)
				{
					if (
						TryGetData<GenericCard>(out var nullCard, unitSlot.HoldingCardId)
						&& nullCard is { } card
						&& card.Charge > 0
						&& battleInstance.State.Turn.Phase == TurnPhase.Attacking
					)
					{
						IsSelected = !IsSelected;
					}
				}
				else
				{
					if (TryGetData<Player>(out var myPlayer, battleInstance.MyPlayerId))
					{
						if (myPlayer?.Team != player?.Team)
						{
							if (battleInstance.State.Turn.Phase == TurnPhase.Attacking)
							{
								Log.Info($"Clicked enemy slot {EntityId}");
								// TODO: Fuck it let's put it here
								// Get all selected slots
								List<UnitSlotNode> selectedSlots = [.. battleInstance.GetEntityNodes<UnitSlotNode>().Where(s => s.IsSelected == true)];
								List<EntityId> selected = selectedSlots.Select(s => s.EntityId).ToList() ?? [];
								selectedSlots.ForEach(s => s.IsSelected = false);
								battleInstance.SendInput(
									new AttackRequestInput(selected, EntityId)
								);
							}
						}
					}
				}

			}
		}
	}


	public void SetGridPosition(UnitSlotPos position)
	{
		TargetPosition = new Vector3(position.X * CellWidth, 0f, -position.Y * CellDepth);
		Position = TargetPosition; // TODO: tween toward TargetPosition instead of snapping.
	}
}
