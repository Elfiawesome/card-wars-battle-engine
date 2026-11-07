using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Rules;

public readonly record struct ContentPointer(
	// Points to a Hard-Coded resource registered in registry
	[property: DataTag("resource")] ResourceId? Resource = null,
	// Data driven
	[property: DataTag("definition")] CompoundTag? Definition = null
);
