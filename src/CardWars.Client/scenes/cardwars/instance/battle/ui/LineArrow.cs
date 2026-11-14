using System;
using Godot;

namespace CardWars.Client.scenes.cardwars.instance.battle;

public partial class LineArrow : Node2D
{
	[Export] public float Direction { get; set; }
	[Export] public float CurveStrength { get; set; } = 0.6f;
	[Export] public float CurveSign { get; set; } = 1f;
	[Export] public float Smoothness { get; set; } = 0.8f;
	[Export] public Vector2 Target { get; set; }

	private Path2D? _path;
	private Line2D? _line;
	private Sprite2D? _arrowHead;
	private Curve2D? _curve;
	private Vector2 _tip;

	public override void _Ready()
	{
		_path = GetNode<Path2D>("Path2D");
		_line = GetNode<Line2D>("Path2D/Line2D");
		_arrowHead = GetNode<Sprite2D>("Path2D/Line2D/Sprite2D");

		_curve = new Curve2D();
		_curve.AddPoint(Vector2.Zero, Vector2.Zero, Vector2.Zero);
		_curve.AddPoint(Vector2.Zero, Vector2.Zero, Vector2.Zero);
		_path.Curve = _curve;
	}

	public override void _Process(double delta)
	{
		if (_line is null || _arrowHead is null || _curve is null) return;

		_tip = _tip.Lerp(Target, 1f - Smoothness);

		var distance = _tip.Length();
		if (distance < 0.001f)
		{
			_line.Points = [];
			return;
		}

		var forward = Vector2.Right.Rotated(Direction);
		var toTip = _tip / distance;
		var sinAngle = forward.Cross(toTip);
		var side = MathF.Sign(sinAngle);
		if (forward.Dot(toTip) < 0f) side = -side;

		var bowDirection = toTip.Orthogonal();
		var bow = bowDirection * side * MathF.Abs(sinAngle) * distance * CurveStrength * CurveSign;
		var control = _tip * 0.5f + bow;
		var handleStart = control * (2f / 3f);
		var handleEnd = (control - _tip) * (2f / 3f);

		_curve.SetPointPosition(1, _tip);
		_curve.SetPointOut(0, handleStart);
		_curve.SetPointIn(1, handleEnd);

		var baked = _curve.GetBakedPoints();
		_line.Points = baked;

		if (baked.Length < 2) return;

		var tip = baked[^1];
		var tangent = tip - baked[^2];
		_arrowHead.Position = tip;
		_arrowHead.Rotation = tangent.Angle();
	}

	public void SetScreenOrigin(CanvasItem parent, Vector2 viewportPoint)
		=> Position = parent.GetGlobalTransformWithCanvas().AffineInverse() * viewportPoint;

	public void SnapTo(Camera3D camera, Node3D target)
	{
		Target = GetGlobalTransformWithCanvas().AffineInverse() * camera.UnprojectPosition(target.GlobalPosition);
		Smoothness = 0.2f;
	}

	public void FollowMouse()
	{
		Smoothness = 0.8f;
		Target = GetLocalMousePosition();
	}
}
