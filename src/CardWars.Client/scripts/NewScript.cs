using Godot;
using CardWars.BattleEngine.Vanilla;

namespace CardWars.Client;

public partial class NewScript : Node
{
	public override void _Ready()
	{
		BattleEngine.BattleEngine be = new();
		VanillaMod mod = new();
		be.LoadMod(mod);
		
		
		GD.Print(be.Registry.BlockHandlers);
	}
}

