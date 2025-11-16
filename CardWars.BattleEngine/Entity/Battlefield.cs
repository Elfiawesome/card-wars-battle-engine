namespace CardWars.BattleEngine.Entity;

public class Battlefield(EntityService service, BattlefieldId id) : Entity<BattlefieldId>(service, id)
{
	public PlayerId OwnerPlayerId { get; set; }
	public HashSet<UnitSlotId> SlotIds { get; set; } = [];
}

public record struct BattlefieldId(Guid Id);