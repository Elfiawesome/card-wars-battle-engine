using CardWars.BattleEngine.Core.Registry;
using CardWars.BattleEngine.Data;

namespace CardWars.BattleEngine.State;

public record struct BehaviourPointer(
	// Points to a Hard-Coded behaviour registered in registry
	ResourceId? BehaviourResource = null,
	// Data driven behaviour to create
	CompoundTag? BehaviourDefinition = null
)
{
	public static implicit operator BehaviourPointer(CompoundTag tag)
	{
		var resource = tag.GetString("resource");
		return new BehaviourPointer(
			BehaviourResource: string.IsNullOrEmpty(resource) ? default : ResourceId.Parse(resource),
			BehaviourDefinition: tag
		);
	}
};