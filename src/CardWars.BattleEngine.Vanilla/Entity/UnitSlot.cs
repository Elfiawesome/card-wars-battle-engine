using CardWars.BattleEngine.State;

namespace CardWars.BattleEngine.Vanilla.Entity;

public class UnitSlot(EntityId id) : IEntity
{
	public EntityId Id { get; init; } = id;
	public EntityId OwnerBattlefieldId { get; set; }
	public EntityId? HoldingCardId { get; set; }

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}