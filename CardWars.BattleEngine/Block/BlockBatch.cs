namespace CardWars.BattleEngine.Block;

public class BlockBatch
{
	public string AnimationId = "";
	public List<IBlock> Actions = [];

	public BlockBatch() { }
	public BlockBatch(IBlock action, string? animationId = null)
	{
		if (animationId != null) { AnimationId = animationId; }
		Actions.Add(action);
	}
	public BlockBatch(ICollection<IBlock> actions, string? animationId = null)
	{
		if (animationId != null) { AnimationId = animationId; }
		Actions.AddRange(actions);
	}
}