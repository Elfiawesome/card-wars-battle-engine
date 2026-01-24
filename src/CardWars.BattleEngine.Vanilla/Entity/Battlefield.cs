using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class Battlefield(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId OwnerPlayerId { get; set; }
	public HashSet<EntityId> UnitSlotIds { get; } = [];

	public int BehaviourPriority => 0;

	public List<BehaviourPointer> GetBehaviours() => [];
}