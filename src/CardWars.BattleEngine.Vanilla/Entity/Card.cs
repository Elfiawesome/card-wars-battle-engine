using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class GenericCard(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId? OwnerPlayerId { get; set; }
	public EntityId? OwnerUnitSlotId { get; set; }

	// Data driven functionality here?
	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}