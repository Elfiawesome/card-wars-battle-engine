using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class TargetHighlight : MeshInstance3D
{
	[Export] public Color GlowColor { get; set; } = new(0.35f, 1f, 0.45f, 1f);
	[Export] public float PulseSpeed { get; set; } = 2.2f;
	[Export] public float MinAlpha { get; set; } = 0.25f;
	[Export] public float MaxAlpha { get; set; } = 0.8f;
	[Export] public float BorderWidth { get; set; } = 0.055f;
	[Export] public float BorderSoftness { get; set; } = 0.015f;
	[Export] public float ShineSpeed { get; set; } = 0.75f;
	[Export] public float HoverBoost { get; set; } = 1.35f;

	private ShaderMaterial? _material;
	private Vector2 _size = Vector2.One;
	private bool _hovered;

	public override void _Ready()
	{
		if (GetSurfaceOverrideMaterial(0) is ShaderMaterial source)
		{
			_material = source;
		}
		ApplySettings();
		Visible = false;
	}

	public void Configure(HighlightBounds bounds)
	{
		_size = bounds.Size;
		Position = bounds.Center;
		Rotation = new Vector3(0f, bounds.Rotation, 0f);
		Mesh = new PlaneMesh { Size = bounds.Size };
		ApplySettings();
	}

	public void SetHighlighted(bool highlighted) => Visible = highlighted;

	public void SetHovered(bool hovered)
	{
		if (_hovered == hovered) return;

		_hovered = hovered;
		ApplySettings();
	}

	private void ApplySettings()
	{
		if (_material is null) return;

		_material.SetShaderParameter("glow_color", GlowColor);
		_material.SetShaderParameter("quad_size", _size);
		_material.SetShaderParameter("pulse_speed", PulseSpeed);
		_material.SetShaderParameter("pulse_min", MinAlpha);
		_material.SetShaderParameter("pulse_max", MaxAlpha);
		_material.SetShaderParameter("border_width", BorderWidth);
		_material.SetShaderParameter("border_softness", BorderSoftness);
		_material.SetShaderParameter("shine_speed", ShineSpeed);
		_material.SetShaderParameter("hover_boost", _hovered ? HoverBoost : 1f);
	}
}
