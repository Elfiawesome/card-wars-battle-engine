using CardWars.BattleEngine.Behaviour;

namespace CardWars.BattleEngine.State.Entities;

public class UnitSlot : IEntity
{
	public EntityId Id { get; init; }
	public EntityId HoldingCard { get; set; }

	public int BehaviourPriority => 0;
	public List<BehvaiourPointer> GetBheaviours() => [];
}