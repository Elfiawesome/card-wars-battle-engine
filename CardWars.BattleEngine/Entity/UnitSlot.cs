namespace CardWars.BattleEngine.Entity;

public class UnitSlot(EntityService service, UnitSlotId id) : Entity<UnitSlotId>(service, id)
{
	
}

public record struct UnitSlotId(Guid Id);