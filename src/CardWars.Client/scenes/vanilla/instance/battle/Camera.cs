using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class Camera : Camera3D
{
	[Export] public Node3D? Target { get; set; } = null;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
