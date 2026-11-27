namespace CardWars.BattleEngine.State;

public interface IEntity
{
	public EntityId Id { get; init; }
	public int BehaviourPriority { get; }
}

public interface IParentableEntity
{
	public EntityId? ParentId { get; }
}
