using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Block;

public record struct BlockBatch(
	List<IBlock> Blocks,
	EntityId? TargetPlayerId = null,
	string AnimationId = ""
);