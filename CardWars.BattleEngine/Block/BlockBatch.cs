using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block;

public class BlockBatch
{
	public string AnimationId = "";
	public List<BlockWrapper> Blocks = [];

	public BlockBatch() { }

	public void AddBlock(IBlock block, HashSet<PlayerId> targetedPlayerId)
	{
		AddBlock(new BlockWrapper(block, targetedPlayerId));
	}

	public void AddBlock(IBlock block)
	{
		AddBlock(new BlockWrapper(block));
	}

	public void AddBlock(IEnumerable<IBlock> blocks)
	{
		blocks.ToList().ForEach(AddBlock);
	}

	public void AddBlock(IEnumerable<IBlock> blocks, HashSet<PlayerId> targetedPlayerId)
	{
		blocks.ToList().ForEach((b) => AddBlock(b, targetedPlayerId));
	}

	public void AddBlock(BlockWrapper blockWrapper)
	{
		Blocks.Add(blockWrapper);
	}
}