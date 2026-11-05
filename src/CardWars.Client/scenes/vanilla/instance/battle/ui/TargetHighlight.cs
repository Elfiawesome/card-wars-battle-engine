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

	private bool _hovered;

	public override void _Ready()
	{
		ApplySettings();
		Visible = false;
	}

	public void Configure(HighlightBounds bounds)
	{
		Position = bounds.Center;
		Rotation = new Vector3(0f, bounds.Rotation, 0f);
		Mesh = new PlaneMesh { Size = bounds.Size };
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
		var material = GetSurfaceOverrideMaterial(0);
		if (material is ShaderMaterial shaderMaterial)
		{
			shaderMaterial.SetShaderParameter("glow_color", GlowColor);
			shaderMaterial.SetShaderParameter("pulse_speed", PulseSpeed);
			shaderMaterial.SetShaderParameter("pulse_min", MinAlpha);
			shaderMaterial.SetShaderParameter("pulse_max", MaxAlpha);
			shaderMaterial.SetShaderParameter("border_width", BorderWidth);
			shaderMaterial.SetShaderParameter("border_softness", BorderSoftness);
			shaderMaterial.SetShaderParameter("shine_speed", ShineSpeed);
			shaderMaterial.SetShaderParameter("hover_boost", _hovered ? HoverBoost : 1f);
		}
	}
}
