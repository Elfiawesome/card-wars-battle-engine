using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.BattleEngine.Block;

public record struct BlockBatch(
	[property: DataTag] List<IBlock> Blocks,
	[property: DataTag] EntityId? TargetPlayerId = null,
	[property: DataTag] string AnimationId = ""
)
{
	[DataTag] public CompoundTag AnimationArgs { get; set; } = new();
}