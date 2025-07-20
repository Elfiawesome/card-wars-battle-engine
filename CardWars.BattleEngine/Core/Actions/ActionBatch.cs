namespace CardWars.BattleEngine.Core.Actions;

public class ActionBatch
{
	public string AnimationId = "";
	public List<ActionData> Actions = [];

	public ActionBatch() { }
	public ActionBatch(string animationId) { AnimationId = animationId; }
	public ActionBatch(List<ActionData> actions) { Actions = actions; }
	public ActionBatch(List<ActionData> actions, string animationId) { Actions = actions; AnimationId = animationId; }
	public ActionBatch(ActionData action) { Actions.Add(action); }
	public ActionBatch(ActionData action, string animationId) { Actions.Add(action); AnimationId = animationId; }
}
