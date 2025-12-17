namespace CardWars.BattleEngine.State;

public class Battlefield(BattlefieldId id) : EntityState<BattlefieldId>(id)
{
	public PlayerId OwnerPlayerId { get; set; }
	public HashSet<UnitSlotId> ControllingUnitSlotIds { get; set; } = [];
}

public record struct BattlefieldId(Guid Id) : IStateId;