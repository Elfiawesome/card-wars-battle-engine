using CardWars.BattleEngine.State.Entity;

namespace CardWars.BattleEngine.Block;

public record class BlockWrapper(
	IBlock Block,
	HashSet<PlayerId>? TargetedPlayerIds = null
);