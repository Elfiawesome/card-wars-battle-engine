using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class TestEntity(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;

	public int BehaviourPriority => 0;

	public List<BehaviourPointer> GetBehaviours()
	{
		return [new("special_staged_behaviour")];
	}
}