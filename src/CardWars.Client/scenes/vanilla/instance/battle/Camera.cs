using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class Camera : Camera3D
{
	[Export] public Node3D? Target { get; set; }
	[Export] public float SmoothSpeed { get; set; } = 4f;
	[Export] public float ZoomSpeed { get; set; } = 5f;
	[Export] public float ZoomStep { get; set; } = 0.2f;
	[Export] public float MinHeight { get; set; } = 2f;
	[Export] public float MaxHeight { get; set; } = 4f;

	public Vector2 PanOffset { get; private set; }

	private Vector2 _freeFocus;
	private float _height;
	private float _pitchDegrees;

	public override void _Ready()
	{
		_freeFocus = new Vector2(GlobalPosition.X, GlobalPosition.Y);
		_height = Mathf.Clamp(GlobalPosition.Z, MinHeight, MaxHeight);

		// How far the camera tilts away from looking straight down
		var forward = -GlobalBasis.Z;
		_pitchDegrees = Mathf.RadToDeg(Mathf.Acos(Mathf.Clamp(-forward.Z, -1f, 1f)));
	}

	public void SetTarget(Node3D? target)
	{
		Target = target;
		PanOffset = Vector2.Zero;
	}

	public void SetFocus(Vector2 focus)
	{
		Target = null;
		_freeFocus = focus;
		PanOffset = Vector2.Zero;
	}

	public void SetPan(Vector2 pan) => PanOffset = pan;

	public void Zoom(float steps)
		=> _height = Mathf.Clamp(_height + steps * ZoomStep, MinHeight, MaxHeight);

	public override void _Process(double delta)
	{
		var weight = (float)(SmoothSpeed * delta);

		if (Target != null && IsInstanceValid(Target))
		{
			var relative = new Transform3D(
				Basis.FromEuler(new Vector3(Mathf.DegToRad(-90f + _pitchDegrees), 0f, 0f)),
				new Vector3(0f, _height, 0f));
			var desired = Target.GlobalTransform * relative;
			desired.Origin += new Vector3(PanOffset.X, PanOffset.Y, 0f);

			GlobalPosition = GlobalPosition.Lerp(desired.Origin, weight);
			GlobalBasis = new Basis(GlobalBasis.GetRotationQuaternion().Slerp(desired.Basis.GetRotationQuaternion(), weight));
		}
		else
		{
			var desired = new Vector3(_freeFocus.X + PanOffset.X, _freeFocus.Y + PanOffset.Y, _height);
			GlobalPosition = GlobalPosition.Lerp(desired, weight);
		}
	}
}
