using System;
using CardWars.BattleEngine.Vanilla.Entity;
using CardWars.Core.Logging;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class UnitSlotNode : Node3D
{
	public const float CellWidth = 0.9f;
	public const float CellDepth = 1.1f;

	public Action? OnMouseEntered;
	public Action? OnMouseExited;
	public Action? OnMouseClicked;

	public Vector3 TargetPosition { get; private set; } = Vector3.Zero;

	public override void _Ready()
	{
		base._Ready();

		// This code
		var collisionBox = GetNode<Area3D>("Area3D");
		collisionBox.InputEvent += onCollisionInputEvent;
		collisionBox.MouseEntered += () => { OnMouseEntered?.Invoke(); Log.Info("Entered"); };
		collisionBox.MouseExited += () => { OnMouseExited?.Invoke(); Log.Info("Exited"); };
	}

	private void onCollisionInputEvent(Node cameraNode, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIndex)
	{
		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.Pressed && inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				Log.Info($"Clicked on {this}");
				OnMouseClicked?.Invoke();
			}
		}
	}


	public void SetGridPosition(UnitSlotPos position)
	{
		TargetPosition = new Vector3(position.X * CellWidth, 0f, -position.Y * CellDepth);
		Position = TargetPosition; // TODO: tween toward TargetPosition instead of snapping.
	}
}
