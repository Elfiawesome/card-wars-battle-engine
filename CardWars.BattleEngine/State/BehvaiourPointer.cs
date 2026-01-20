using CardWars.BattleEngine.Core.Registry;

namespace CardWars.BattleEngine.State;

public record struct BehaviourPointer(
	// Points to a Hard-Coded behaviour registered in registry
	ResourceId? BehaviourResource = null,
	// Data driven behaviour to create
	object? BehaviourDefinition = null
);