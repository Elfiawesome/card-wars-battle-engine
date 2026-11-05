using System;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Data.Tags;
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
		collisionBox.InputEvent += onCollisionInputEvent;
		collisionBox.MouseEntered += () => { OnMouseEntered?.Invoke(); };
		collisionBox.MouseExited += () => { OnMouseExited?.Invoke(); };
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (CardAnchor is not null)
		{
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


	private void onCollisionInputEvent(Node cameraNode, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIndex)
	{
		if (@event is not InputEventMouseButton inputEventMouseButton
			|| !inputEventMouseButton.Pressed
			|| inputEventMouseButton.ButtonIndex != MouseButton.Left) { return; }

		if (BattleInstance is { } battleInstance)
		{
			if (
				// Check UnitSlot exists
				TryGetData<UnitSlot>(out var unitSlot)
				&& unitSlot?.OwnerBattlefieldId is { } battlefieldId
				// Check Battlefield exists
				&& TryGetData<Battlefield>(out var battlefield, battlefieldId)
				&& battlefield?.OwnerPlayerId is { } ownerPlayerId
				// Check Player exists
				&& TryGetData<Player>(out var player, battlefield.OwnerPlayerId)
			)
			{
				if (battleInstance.MyPlayerId == ownerPlayerId)
				{
					OnMouseClicked?.Invoke();
					if (CardAnchor?.GetChildCount() > 0) { IsSelected = !IsSelected; }
				}
				else
				{
					if (TryGetData<Player>(out var myPlayer, battleInstance.MyPlayerId))
					{
						if (myPlayer?.Team != player?.Team)
						{
							Log.Info($"Clicked enemy slot {EntityId}");
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
