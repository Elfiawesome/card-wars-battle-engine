using System;
using CardWars.BattleEngine.Vanilla.Entity;
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
	}

	public void SetSelected(bool selected) => IsSelected = selected;

	public void SetGridPosition(UnitSlotPos position)
	{
		TargetPosition = new Vector3(position.X * CellWidth, 0f, -position.Y * CellDepth);
		Position = TargetPosition; // TODO: tween toward TargetPosition instead of snapping.
	}
}
