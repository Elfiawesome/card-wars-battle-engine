using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class GenericCard(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId ParentUnitSlotId { get; set; }
	public EntityId ParentPlayerId { get; set; }

	// Data driven functionality here?
	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}