using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Block;

public record struct BlockBatch(
	[property: DataTag] List<IBlock> Blocks,
	[property: DataTag] EntityId? TargetPlayerId = null,
	[property: DataTag] string AnimationId = ""
)
{
	[DataTag] public List<string> AnimationArgs { get; set; } = [];
}