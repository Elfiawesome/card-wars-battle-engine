namespace CardWars.BattleEngine.Entity;

public class UnitSlot(EntityService service, UnitSlotId id) : Entity<UnitSlotId>(service, id)
{
	public BattlefieldId OwnerBattlefieldId { get; set; }
	public UnitCardId UnitCardId { get; set; }
	public UnitPosition Position { get; set; }
}

public record struct UnitPosition(int X, int Y);
public record struct UnitSlotId(Guid Id);