namespace CardWars.BattleEngine.Core.Actions;

public class ActionBatch
{
	public string AnimationId = "";
	// public List<IActionData> Actions = [];
	public List<ActionContainer> Actions = [];

	public ActionBatch() { }
	public ActionBatch(string animationId) { AnimationId = animationId; }
	public ActionBatch(List<ActionContainer> actions, string animationId = "") { Actions = actions; AnimationId = animationId; }
	public ActionBatch(ActionContainer action, string animationId = "") { Actions.Add(action); AnimationId = animationId; }

	public ActionBatch(IActionData action, string animationId = "") { Actions.Add(new ActionContainer() { Action = action }); AnimationId = animationId; }
	public ActionBatch(List<IActionData> actions, string animationId = "")
	{
		Actions = actions.Select((a) => new ActionContainer(){Action = a}).ToList();
		AnimationId = animationId;
	}

	// public ActionBatch(List<IActionData> actions) { Actions = actions; }
	// public ActionBatch(List<IActionData> actions, string animationId) { Actions = actions; AnimationId = animationId; }
	// public ActionBatch(IActionData action) { Actions.Add(action); }
	// public ActionBatch(IActionData action, string animationId) { Actions.Add(action); AnimationId = animationId; }
}
