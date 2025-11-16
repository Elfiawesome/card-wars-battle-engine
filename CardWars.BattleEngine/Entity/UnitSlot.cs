namespace CardWars.BattleEngine.Entity;

public class UnitSlot(EntityService service, UnitSlotId id) : Entity<UnitSlotId>(service, id)
{
	public BattlefieldId OwnerBattlefieldId { get; set; }
	public UnitCardId UnitCardId { get; set; }
}

public record struct UnitSlotId(Guid Id);