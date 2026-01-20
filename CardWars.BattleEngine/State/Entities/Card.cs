using CardWars.BattleEngine.Behaviour;

namespace CardWars.BattleEngine.State.Entities;

public class GenericCard : IEntity
{
	public EntityId Id { get; init; }
	public EntityId ParentUnitSlotId { get; set; }
	public EntityId ParentPlayerId { get; set; }

	// Data driven functionality here?
	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBheaviours() => [];
}