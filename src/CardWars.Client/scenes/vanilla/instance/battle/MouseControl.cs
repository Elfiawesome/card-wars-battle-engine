using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class MouseControl : Control
{
	[Export] public Camera? CameraNode { get; set; }
	[Export] public Node3D? Target { get; set; }

	private bool _dragging;
	private Vector2 _dragStartMouse;
	private Vector2 _dragStartPan;

	public override void _Ready()
	{
		if (CameraNode != null && Target != null) CameraNode.SetTarget(Target);
	}

	public void FocusOn(Node3D? target)
		=> CameraNode?.SetTarget(target);

	public override void _GuiInput(InputEvent @event)
	{
		if (CameraNode == null) return;

		switch (@event)
		{
			case InputEventMouseButton button:
				if (button.ButtonIndex == MouseButton.WheelDown) CameraNode.Zoom(1f);
				if (button.ButtonIndex == MouseButton.WheelUp) CameraNode.Zoom(-1f);

				if (button.ButtonIndex == MouseButton.Left)
				{
					if (button.Pressed)
					{
						_dragging = true;
						_dragStartMouse = button.Position;
						_dragStartPan = CameraNode.PanOffset;
					}
					else
					{
						_dragging = false;
					}
				}
				break;

			case InputEventMouseMotion motion when _dragging:
				var start = GroundPoint(CameraNode, _dragStartMouse);
				var current = GroundPoint(CameraNode, motion.Position);
				if (start == null || current == null) return;

				var delta = current.Value - start.Value;
				CameraNode.SetPan(_dragStartPan - new Vector2(delta.X, delta.Y));
				break;
		}
	}

	private static Vector3? GroundPoint(Camera3D camera, Vector2 screenPosition)
	{
		var from = camera.ProjectRayOrigin(screenPosition);
		var direction = camera.ProjectRayNormal(screenPosition);
		if (Mathf.IsZeroApprox(direction.Z)) return null;

		var distance = -from.Z / direction.Z;
		if (distance < 0f) return null;

		return from + direction * distance;
	}
}
