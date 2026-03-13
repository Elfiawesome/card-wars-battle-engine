using CardWars.BattleEngine.State;
using CardWars.Core.Data;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType()]
public class UnitSlot(EntityId id) : IEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId? OwnerBattlefieldId { get; set; }
	[DataTag] public EntityId? HoldingCardId { get; set; }
	[DataTag] public UnitSlotPos Position { get; set; } = new UnitSlotPos(0, 0);

	public int BehaviourPriority => 0;
	public List<BehaviourPointer> GetBehaviours() => [];
}

public record struct UnitSlotPos(
	[property: DataTag] int X,
	[property: DataTag] int Y
);