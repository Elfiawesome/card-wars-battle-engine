using Godot;

namespace CardWars.Client;

public partial class GameSession : Node
{
	public override void _Ready()
	{
		GD.Print(new BattleEngine.BattleEngine());
	}

	public override void _Process(double delta)
	{
	}
}
