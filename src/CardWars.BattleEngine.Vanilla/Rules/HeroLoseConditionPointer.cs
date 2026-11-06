using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Rules;

public record struct HeroLoseConditionPointer(
	[property: DataTag("resource")] ResourceId? Resource = null,
	[property: DataTag("definition")] CompoundTag? Definition = null
);
