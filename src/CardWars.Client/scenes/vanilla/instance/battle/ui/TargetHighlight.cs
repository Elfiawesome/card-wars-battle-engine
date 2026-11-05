using System;
using CardWars.Core.Logging;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class TargetHighlight : MeshInstance3D
{
	[Export] public Color GlowColor { get; set; } = new(0.35f, 1f, 0.45f, 1f);
	[Export] public float PulseSpeed { get; set; } = 2.2f;
	[Export] public float MinAlpha { get; set; } = 0.18f;
	[Export] public float MaxAlpha { get; set; } = 0.55f;
	[Export] public float ScalePulse { get; set; } = 0.04f;
	[Export] public float HoverBoost { get; set; } = 1.35f;

	private StandardMaterial3D? _material;
	private Vector3 _baseScale;
	private float _time;
	private bool _hovered;

	public override void _Ready()
	{
		_baseScale = Scale;

		_material = new StandardMaterial3D
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
			BlendMode = BaseMaterial3D.BlendModeEnum.Add,
			CullMode = BaseMaterial3D.CullModeEnum.Disabled,
			EmissionEnabled = true,
			Emission = GlowColor,
			EmissionEnergyMultiplier = 2f
		};

		MaterialOverride = _material;
		Visible = false;
		SetProcess(false);
	}

	public override void _Process(double delta)
	{
		if (_material is null) return;

		_time += (float)delta * PulseSpeed;
		var pulse = 0.5f + 0.5f * MathF.Sin(_time);
		var boost = _hovered ? HoverBoost : 1f;
		var alpha = Mathf.Lerp(MinAlpha, MaxAlpha, pulse) * boost;

		_material.AlbedoColor = new Color(GlowColor.R, GlowColor.G, GlowColor.B, alpha);
		_material.EmissionEnergyMultiplier = Mathf.Lerp(1.2f, 2.8f, pulse) * boost;
		Scale = _baseScale * (1f + ScalePulse * pulse * boost);
	}

	public void SetHighlighted(bool highlighted)
	{
		Visible = highlighted;
		SetProcess(highlighted);
	}

	public void SetHovered(bool hovered) => _hovered = hovered;
}
