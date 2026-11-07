using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Rules;

public readonly record struct ContentPointer(
	[property: DataTag("resource")] ResourceId? Resource = null,
	[property: DataTag("definition")] CompoundTag? Definition = null
);
