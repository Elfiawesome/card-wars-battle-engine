using CardWars.BattleEngine.State;

namespace CardWars.BattleEngineVanilla.Entity;

public class TestEntity(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;

	public int BehaviourPriority => 1;

	public List<BehaviourPointer> GetBehaviours()
	{
		return [new(){BehaviourResource = "example_behaviour"}];
	}
}