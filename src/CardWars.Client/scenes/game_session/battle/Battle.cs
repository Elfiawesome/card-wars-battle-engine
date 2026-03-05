using System;
using CardWars.BattleEngine.Vanilla;
using Godot;

namespace CardWars.Client;

public partial class Battle : Node2D
{
	public BattleEngine.BattleEngine battleEngine = new();
	
	public override void _Ready()
	{
		// Simulate we create the battle engine here first to test out client viewing
		battleEngine.LoadMod(new VanillaMod());
	}

	public override void _Process(double delta)
	{
	}
}
