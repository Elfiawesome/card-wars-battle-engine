using CardWars.Core.Data;
using CardWars.BattleEngine.State;
using CardWars.Core.Data.Attributes;
using CardWars.Core.Registry;

namespace CardWars.BattleEngine.Vanilla.Entity;

[DataTagType("battle/entity/unit_slot")]
public class UnitSlot(EntityId id) : IEntity, IParentableEntity
{
	[DataTag] public EntityId Id { get; init; } = id;
	[DataTag] public EntityId? OwnerBattlefieldId { get; set; }
	[DataTag] public EntityId? HoldingCardId { get; set; }
	[DataTag] public UnitSlotPos Position { get; set; } = new UnitSlotPos(0, 0);
	public EntityId? ParentId => OwnerBattlefieldId;

	public int BehaviourPriority => 0;
	public List<ContentPointer> GetBehaviours(DataTagContext dataTags) => [];
}

public record struct UnitSlotPos(
	[property: DataTag] int X,
	[property: DataTag] int Y
);