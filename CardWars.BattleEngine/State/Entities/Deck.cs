using CardWars.BattleEngine.Behaviour;

namespace CardWars.BattleEngine.State.Entities;

public class Deck : IEntity
{
	public EntityId Id { get; init; }
	public HashSet<Guid> List = [];

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}