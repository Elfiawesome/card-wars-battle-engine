using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class Deck(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public HashSet<Guid> List = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}