namespace CardWars.BattleEngine.State;

public class UnitSlot(UnitSlotId id) : EntityState<UnitSlotId>(id)
{
	public BattlefieldId OwnerBattlefieldId;
}

public record struct UnitSlotId(Guid Id) : EntityId;