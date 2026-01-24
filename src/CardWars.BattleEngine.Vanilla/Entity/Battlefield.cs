using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class Battlefield(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId ParentPlayerId { get; set; }

	public int BehaviourPriority => 0;

	public List<BehaviourPointer> GetBehaviours() => [];
}