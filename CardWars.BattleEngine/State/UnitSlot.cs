namespace CardWars.BattleEngine.State;

public class UnitSlot(UnitSlotId id) : EntityState<UnitSlotId>(id)
{
	public BattlefieldId OwnerBattlefieldId { get; set; }
}

public record struct UnitSlotId(Guid Id) : IStateId;