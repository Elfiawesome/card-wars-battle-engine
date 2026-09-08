using CardWars.Client.scenes.core.game_session;
using CardWars.Vanilla.Shared;
using Godot;

namespace CardWars.Client.scenes.vanilla.instance.battle;

public partial class BattleInstance : ClientInstance
{
	public Control? UI;
	public HandManager? HandManager;

	public override void _Ready()
	{
		UI = GetNode<Control>("UI");
		HandManager = GetNode<HandManager>("UI/HandManager");
		HandManager.BattleInstance = this;
	}

	public void CreateBattlefield()
	{
		// var battlefield = registry?.GameObjects.Instantiate<Battlefield>(Constant.Battlefield);
		// if (battlefield == null) { return; }
		// AddChild(battlefield);
	}
}
