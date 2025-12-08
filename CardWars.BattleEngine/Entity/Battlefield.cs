namespace CardWars.BattleEngine.Entity;

public class Battlefield(BattlefieldId id) : Entity<BattlefieldId>(id)
{
	public PlayerId OwnerPlayerId { get; set; }
	public HashSet<UnitSlotId> UnitSlotIds { get; set; } = [];
}

public record struct BattlefieldId(Guid Id);