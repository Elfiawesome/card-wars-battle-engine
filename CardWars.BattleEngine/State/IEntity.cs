namespace CardWars.BattleEngine.State;

public interface IEntity
{
	public EntityId Id { get; init; }
	public int BehaviourPriority { get; }

	public List<BehaviourPointer> GetBheaviours();
}