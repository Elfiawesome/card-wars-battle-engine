namespace CardWars.BattleEngine.Core.Actions;

public abstract class GameActionBatch
{
	public string AnimationId { get; set; } = "";
	public List<GameAction> Actions = [];

	public GameActionBatch(List<GameAction> actions)
	{
		Actions = actions;
	}

	public GameActionBatch(GameAction[] actions)
	{
		Actions = actions.ToList();
	}

	public GameActionBatch(GameAction action)
	{
		Actions.Add(action);
	}
}