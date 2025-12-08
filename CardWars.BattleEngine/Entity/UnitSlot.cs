namespace CardWars.BattleEngine.Entity;

public class UnitSlot(UnitSlotId id) : Entity<UnitSlotId>(id)
{
	public BattlefieldId OwnerBattlefieldId { get; set; }
	public UnitCardId UnitCardId { get; set; }
	public UnitPosition Position { get; set; }
}

public record struct UnitPosition(int X, int Y);
public record struct UnitSlotId(Guid Id);