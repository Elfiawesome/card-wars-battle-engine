using Godot;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public override void _Ready()
	{
		var scene = GD.Load<PackedScene>("res://scenes/game_session/battle/battle.tscn");
		AddChild(scene.Instantiate<Node2D>());
	}

	public override void _Process(double delta)
	{
	}
}
