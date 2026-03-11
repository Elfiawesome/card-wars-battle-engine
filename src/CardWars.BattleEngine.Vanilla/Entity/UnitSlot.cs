using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType()]
public class UnitSlot(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId OwnerBattlefieldId { get; set; }
	[DataTag] public EntityId? HoldingCardId { get; set; }

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}