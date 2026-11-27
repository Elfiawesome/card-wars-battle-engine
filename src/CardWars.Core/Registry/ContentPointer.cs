using CardWars.Core.Data.Attributes;
using CardWars.Core.Data.Tags;

namespace CardWars.Core.Registry;

public readonly record struct ContentPointer(
	// Points to a Hard-Coded resource registered in registry
	[property: DataTag("resource")] ResourceId? Resource = null,
	// Data driven
	[property: DataTag("definition")] CompoundTag? Definition = null
);
