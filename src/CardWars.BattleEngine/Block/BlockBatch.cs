namespace CardWars.BattleEngine.Block;

public record struct BlockBatch(
	List<IBlock> Blocks,
	Guid TargetPlayerId,
	string AnimationId = ""
);