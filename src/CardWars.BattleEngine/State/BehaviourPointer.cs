using CardWars.Core.Data;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.State;

public record struct BehaviourPointer(
	// Points to a Hard-Coded behaviour registered in registry
	[property: DataTag("resource")] ResourceId? BehaviourResource = null,
	// Data driven behaviour to create
	[property: DataTag("definition")] CompoundTag? BehaviourDefinition = null
);