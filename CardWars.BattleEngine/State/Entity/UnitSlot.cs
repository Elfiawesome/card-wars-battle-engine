namespace CardWars.BattleEngine.State.Entity;

public class UnitSlot(UnitSlotId id) : EntityState<UnitSlotId>(id)
{
	public BattlefieldId OwnerBattlefieldId { get; set; }
	public UnitCardId? HoldingCardId { get; set; } = null;
}

public record struct UnitSlotId(Guid Id) : IStateId;