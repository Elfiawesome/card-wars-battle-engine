namespace CardWars.BattleEngine.Block;

public class BlockBatch
{
	public string AnimationId = "";
	public List<IBlock> Blocks = [];

	public BlockBatch() { }
	public BlockBatch(IBlock block, string? animationId = null)
	{
		if (animationId != null) { AnimationId = animationId; }
		Blocks.Add(block);
	}
	public BlockBatch(ICollection<IBlock> blocks, string? animationId = null)
	{
		if (animationId != null) { AnimationId = animationId; }
		Blocks.AddRange(blocks);
	}
}